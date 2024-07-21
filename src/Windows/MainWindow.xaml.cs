using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Shapes;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DQB2ChunkEditor.Controls;
using DQB2ChunkEditor.Models;
using Microsoft.VisualBasic;
using Microsoft.Win32;

namespace DQB2ChunkEditor.Windows;

public partial class MainWindow : Window
{
    public List<Tile> TileList { get; } = new();
    public static ObservableCollection<ComboBoxTile> TileComboBoxList { get; set; } = new();
    public ObservableProperty<Tile> SelectedTile { get; set; } = new();
    public Tile selectedTileDrop;
    public ObservableProperty<LayerTile> SelectedLayerTile { get; set; } = new();
    public List<ItemData> ItemDataList { get; set; } = new();
    public SelectionPencilClass SelectedArea { get; set; } = new();
    public ObservableProperty<short> ChunkValue { get; set; } = new() { Value = 0};
    public ObservableProperty<byte> LayerValue { get; set; } = new() { Value = 0 };

    public MainWindow()
    {
        InitializeComponent();
        CreateDefaultTiles();
        CreateComboBoxTiles();
        DataContext = this;
        NotSupported.Visibility = Visibility.Collapsed;
    }

    /// <summary>
    /// Creates the default tile grid used for the chunk layers and their click event
    /// </summary>
    private void CreateDefaultTiles()
    {
        try
        {
            for (short i = 0; i < 1024; i++) // layers are 32x32
            {
                var layerTile = new LayerTile
                {
                    Id = i,
                    Tile = new ObservableProperty<Tile>
                    {
                        Value = new Tile()
                    }
                };

                layerTile.TileButton.Click += (_, _) => { LayerTile_OnClick(layerTile); };

                LayerTiles.Children.Add(layerTile);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    /// <summary>
    /// Creates the dropdown selection tiles for changing a selected tile
    /// </summary>
    private void CreateComboBoxTiles()
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            };

            var json = File.ReadAllText(@"Data\Tiles.json");

            var tiles = JsonSerializer.Deserialize<TileList>(json, options);

            for (var i = 0; i < tiles!.Tiles.Count; i++)
            {
                TileComboBoxList.Add(new ComboBoxTile
                {
                    Id = i,
                    Tile = tiles.Tiles[i]
                });

                TileList.Add(tiles.Tiles[i]);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void RefreshTiles(short chunk, byte layer)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ChunkEditor.Filename))
            {
                return;
            }

            for (short i = 0; i < 1024; i++)
            {
                var blockId = ChunkEditor.GetBlockValue(chunk, layer, i);
                var TileValue = TileList.FirstOrDefault(t => t.Id % 2048 == blockId % 2048) ?? TileList[0];
                TileValue.Id = (short)blockId;
                ((LayerTile)LayerTiles.Children[i]).Tile.Value = TileValue;
            }
            
            ItemDataList = new();
            for (short i = 0; i < ChunkEditor.ItemCount; i++)
            {
                var ChunkItem = ChunkEditor.GetItemChunk((uint)(0x150E7D1+i*4));
                if((short)ChunkItem == chunk){
                    var ItemData = new ItemData((uint)( 0x150E7D1 + i * 4));
                    if(ItemData.PosY == (uint)layer){
                        ItemDataList.Add(ItemData);
                    }
                }
                //var blockId = ChunkEditor.GetBlockValue(chunk, layer, i);
                //var TileValue = TileList.FirstOrDefault(t => t.Id % 2048 == blockId % 2048) ?? TileList[0];
                //TileValue.Id = (short)blockId;
                //((LayerTile)LayerTiles.Children[i]).Tile.Value = TileValue;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    /// <summary>
    /// Here is the selection handler with the magic pencil. First is the line.
    /// </summary>

    private void DrawLine(double x0, double y0, double x1, double y1, List<Line> List,bool yellow)
        {
            Line line = new Line
            {
                X1 = x0,
                Y1 = y0,
                X2 = x1,
                Y2 = y1,
                Stroke = Brushes.Orange,
                StrokeThickness = 5,
            };
            if(yellow) line.Stroke = Brushes.Yellow;

            SelectionCanvas.Children.Add(line);
            List.Add(line);
        }

    /// <summary>
    /// Erases lines.
    /// </summary>
    private void EraseRect_OnClick(Object sender, RoutedEventArgs e)
    {
        EraseLine(SelectedArea.EndList);
        EraseLine(SelectedArea.BegList);
        EraseLine(SelectedArea.RectList);
        SelectedArea.TileIdBeg = -1;
        SelectedArea.TileIdEnd = -1;
        if (PencilButton.IsChecked == true)
        {
            overflowCheckboxName.Visibility = Visibility.Collapsed;
            NotSupported.Visibility = Visibility.Visible;
        }
        else
        {
            overflowCheckboxName.Visibility = Visibility.Visible;
            NotSupported.Visibility = Visibility.Collapsed;
        }
    }
    private void EraseLine(List<Line> List)
    {
        foreach (var line in List)
            SelectionCanvas.Children.Remove(line);
        List.Clear();
    }
    /// <summary>
    /// Completes selection and rotates to the orientation chosen.
    /// </summary>
    private void DrawRectangle(double ActualSize)
        {
            var x0 = SelectedArea.x0 * ActualSize;
            var y0 = SelectedArea.y0 * ActualSize;
            var x1 = SelectedArea.x1 * ActualSize;
            var y1 = SelectedArea.y1 * ActualSize;
            var changex = false;
            var changey = false;

        EraseLine(SelectedArea.EndList);
        EraseLine(SelectedArea.BegList);
        if (x0 > x1)
        {
            changex = true;
            DrawLine(x0 + ActualSize, y0, x0 + ActualSize, y0 + ActualSize, SelectedArea.BegList, false);
            DrawLine(x1, y1 + ActualSize, x1, y1, SelectedArea.EndList, false);
        }
        else
        {
            DrawLine(x0, y0, x0, y0 + ActualSize, SelectedArea.BegList, false);
            DrawLine(x1 + ActualSize, y1 + ActualSize, x1 + ActualSize, y1, SelectedArea.EndList, false);
        }
        if (y0 > y1)
        {
            changey = true;
            DrawLine(x0, y0 + ActualSize, x0 + ActualSize, y0 + ActualSize, SelectedArea.BegList, false);
            DrawLine(x1 + ActualSize, y1, x1, y1, SelectedArea.EndList, false);
        }
        else
        {
            DrawLine(x0, y0, x0 + ActualSize, y0, SelectedArea.BegList, false);
            DrawLine(x1 + ActualSize, y1+ ActualSize, x1, y1+ ActualSize, SelectedArea.EndList, false);
        }

        if (changey == changex){
                if(changex == true){
                    var xa = x0; x0 = x1;  x1 = xa;
                        xa = y0; y0 = y1;  y1 = xa;
                }
                DrawLine(x0+ActualSize,y0,x1+ActualSize,y0,SelectedArea.RectList,true);
                DrawLine(x0,y1+ActualSize,x1,y1+ActualSize,SelectedArea.RectList,true);
                DrawLine(x0,y0+ActualSize,x0,y1+ActualSize,SelectedArea.RectList,true);
                DrawLine(x1+ActualSize,y0,x1+ActualSize,y1,SelectedArea.RectList,true);

            }
            else{
                if(changex == true){
                    var xa = x0; x0 = x1;  x1 = xa;
                        xa = y0; y0 = y1;  y1 = xa;
                }
                DrawLine(x0+ActualSize,y0+ActualSize,x1+ActualSize,y0+ActualSize,SelectedArea.RectList,true);
                DrawLine(x0,y1,x1,y1,SelectedArea.RectList,true);
                DrawLine(x0,y0,x0,y1,SelectedArea.RectList,true);
                DrawLine(x1+ActualSize,y0+ActualSize,x1+ActualSize,y1+ActualSize,SelectedArea.RectList,true);
            }
        }

    /// <summary>
    /// Event handler for when a layer tile is clicked.
    /// </summary>
    private void LayerTile_OnClick(LayerTile layerTile)
    {
        try
        {
            if (layerTile.Tile.Value == null || string.IsNullOrWhiteSpace(ChunkEditor.Filename))
            {
                return;
            }

            SelectedLayerTile.Value = layerTile;
            // if the select button is check, update the dropdown
            if (SelectButton.IsChecked == true)
            {
                TileComboBox.SelectedIndex = (short)TileComboBoxList.FirstOrDefault(t => t.Tile.Id == layerTile.Tile.Value.Id)!.Id;
                // This nonsense rigth here took me too long to figure out do not judge the spaguetti.
                layerTile.Tile.Value.Id = (short)ChunkEditor.GetBlockValue(ChunkValue.Value, LayerValue.Value, SelectedLayerTile.Value.Id);
                SelectedTile.Value = layerTile.Tile.Value;
                
                overflowCheckboxName.IsChecked = SelectedTile.Value.Overflow;
                
            }
            // If it's the paste button just update the tile with the current selected value
            else if(PasteButton.IsChecked == true)
            {
                ((LayerTile)LayerTiles.Children[SelectedLayerTile.Value!.Id]).Tile.Value = SelectedTile.Value;
                ChunkEditor.SetBlockValue(ChunkValue.Value, LayerValue.Value, SelectedLayerTile.Value.Id, SelectedTile.Value.Id);
            }
            // If it's the select button do select logic
            else if(PencilButton.IsChecked == true){
                var coordLine = ((LayerTile)LayerTiles.Children[SelectedLayerTile.Value!.Id]).ActualHeight;
                var x0 = (SelectedLayerTile.Value.Id%32) * coordLine;
                var y0 = (SelectedLayerTile.Value.Id/32) * coordLine;

                if(SelectedArea.TileIdBeg == SelectedLayerTile.Value.Id) {

                    SelectedArea.TileIdBeg = -1;
                    EraseLine(SelectedArea.BegList);
                }
                else if(SelectedArea.TileIdEnd == SelectedLayerTile.Value.Id){

                    SelectedArea.TileIdEnd = -1;
                    EraseLine(SelectedArea.EndList);
                }
                else if(SelectedArea.TileIdBeg == -1){

                    DrawLine(x0, y0, x0, y0+coordLine,SelectedArea.BegList,false);
                    DrawLine(x0, y0, x0+coordLine, y0,SelectedArea.BegList,false);
                    SelectedArea.TileIdBeg = SelectedLayerTile.Value.Id;
                }
                else{
                    EraseLine(SelectedArea.EndList);
                    EraseLine(SelectedArea.RectList);
                    DrawLine(x0+coordLine, y0+coordLine, x0, y0+coordLine,SelectedArea.EndList,false);
                    DrawLine(x0+coordLine, y0+coordLine, x0+coordLine, y0,SelectedArea.EndList,false);
                    SelectedArea.TileIdEnd = SelectedLayerTile.Value.Id;
                }
                if(SelectedArea.TileIdEnd > -1 && SelectedArea.TileIdBeg > -1) DrawRectangle(coordLine);
                else EraseLine(SelectedArea.RectList);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    /// <summary>
    /// Event handler for when the checkbox for overflow gets clicked by the user. Updates the selected tile.
    /// </summary>
    private void UpdateIdSelected_OnClick(object sender, RoutedEventArgs e){
        try
        {
            var change = ((LayerTile)LayerTiles.Children[SelectedLayerTile.Value.Id]).Tile.Value;
            
            if (overflowCheckboxName.IsChecked == true) 

                change.Id = (short)(change.ListId+2048);

            else

                change.Id = (short)(change.ListId);

            ((LayerTile)LayerTiles.Children[SelectedLayerTile.Value.Id]).Tile.Value = change;

            SelectedTile.Value = change;

            ChunkEditor.SetBlockValue(ChunkValue.Value, LayerValue.Value, SelectedLayerTile.Value.Id, change.Id);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

    }

    /// <summary>
    /// Event handler for when a new tile is selected from the dropdown. Updates the selected tile.
    /// </summary>
    private void TileComboBox_OnSelectionChange(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (SelectedLayerTile.Value == null)
            {
                return;
            }

            selectedTileDrop = ((ComboBoxTile)e.AddedItems[0]!).Tile;

            SelectedTile.Value = selectedTileDrop;

            ((LayerTile)LayerTiles.Children[SelectedLayerTile.Value.Id]).Tile.Value = selectedTileDrop;

            if(PencilButton.IsChecked == true){
                ChunkEditor.SetAreaValue(ChunkValue.Value, LayerValue.Value, SelectedArea.TileIdBeg,SelectedArea.TileIdEnd, selectedTileDrop.Id);
                RefreshTiles(ChunkValue.Value, LayerValue.Value);
            }
            ChunkEditor.SetBlockValue(ChunkValue.Value, LayerValue.Value, SelectedLayerTile.Value.Id, selectedTileDrop.Id);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    /// <summary>
    /// Opens a dialog for map file
    /// </summary>
    private void OpenFile_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "STGDAT*.BIN|STGDAT*.BIN"
            };

            if (openFileDialog.ShowDialog() == false)
            {
                return;
            }

            ChunkEditor.LoadFile(openFileDialog.FileName);

            // if we are above the max chunk, reset to the highest chunk
            if (ChunkValue.Value > ChunkEditor.ChunkCount)
            {
                ChunkValue.Value = ChunkEditor.ChunkCount;
            }

            RefreshTiles(ChunkValue.Value, LayerValue.Value);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            MessageBox.Show(ex.Message, "Failed to open file", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Saves the current map bytes to file
    /// </summary>
    private void SaveFile_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ChunkEditor.Filename))
            {
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "*.BIN|*.BIN",
                FileName = "STGDAT" + Convert.ToString(ChunkEditor.Island)
            };
            if (saveFileDialog.ShowDialog() == false)
            {
                return;
            }
            ChunkEditor.Filename = saveFileDialog.FileName;

            ChunkEditor.SaveFile();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            MessageBox.Show(ex.Message, "Failed to save file", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Saves the current map bytes to file
    /// </summary>
    private void ExportFile_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ChunkEditor.Filename))
            {
                return;
            }
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "*.BINE|*.BINE",
                FileName = "STGDAT" + Convert.ToString(ChunkEditor.Island)
            };

            if (saveFileDialog.ShowDialog() == false)
            {
                return;
            }

            ChunkEditor.ExportFile(saveFileDialog.FileName);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            MessageBox.Show(ex.Message, "Failed to export file", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ImportFile_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var importFileDialog = new OpenFileDialog
            {
                Filter = "*.BINE|*.BINE"
            };

            if (importFileDialog.ShowDialog() == false)
            {
                return;
            }

            ChunkEditor.ImportFile(importFileDialog.FileName);

            // if we are above the max chunk, reset to the highest chunk
            if (ChunkValue.Value > ChunkEditor.ChunkCount)
            {
                ChunkValue.Value = ChunkEditor.ChunkCount;
            }

            RefreshTiles(ChunkValue.Value, LayerValue.Value);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            MessageBox.Show(ex.Message, "Failed to import file", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Change the selected chunk area. Updates the layer tiles.
    /// </summary>
    private void ChunkInput_OnMouseUp(Object sender, MouseButtonEventArgs e)
    {
        try
        {
            var inputValueDialog = new InputValue
            {
                MaxValue = ChunkEditor.ChunkCount - 1,
                CurrentValue = ChunkValue.Value
            };

            if (inputValueDialog.ShowDialog() == false ||
                !short.TryParse(inputValueDialog.ResponseText, out var value))
            {
                return;
            }

            TrySetChunk(value);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    /// <summary>
    /// Change the selected layer for a chunk. Updates the layer tiles.
    /// </summary>
    private void LayerInput_OnMouseUp(Object sender, MouseButtonEventArgs e)
    {
        try
        {
            var inputValueDialog = new InputValue
            {
                CurrentValue = LayerValue.Value
            };

            if (inputValueDialog.ShowDialog() == false ||
                !byte.TryParse(inputValueDialog.ResponseText, out var value))
            {
                return;
            }

            TrySetLayer(value);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private void AddChunk_OnClick(Object sender, RoutedEventArgs e)
    {
        TrySetChunk((short)(ChunkValue.Value + 1));
    }

    private void SubChunk_OnClick(Object sender, RoutedEventArgs e)
    {
        TrySetChunk((short)(ChunkValue.Value - 1));
    }

    private void AddLayer_OnClick(Object sender, RoutedEventArgs e)
    {
        TrySetLayer((byte)(LayerValue.Value + 1));
    }

    private void SubLayer_OnClick(Object sender, RoutedEventArgs e)
    {
        TrySetLayer((byte)(LayerValue.Value - 1));
    }

    private void TrySetChunk(short value)
    {
        if (value < 0 || value >= ChunkEditor.ChunkCount)
        {
            return;
        }

        ChunkValue.Value = value;

        RefreshTiles(ChunkValue.Value, LayerValue.Value);
    }

    private void TrySetLayer(byte value)
    {
        if (value is < 0 or > 95)
        {
            return;
        }

        LayerValue.Value = value;

        RefreshTiles(ChunkValue.Value, LayerValue.Value);
    }

    private void ReplaceBlocks_OnClick(Object sender, RoutedEventArgs e){
    try
        {
            var replaceBlockWindow = new ReplaceBlock
            {
                FirstId = -1,
                SecondId = -1
            };

            if (replaceBlockWindow.ShowDialog() == false)
            {
                return;
            }
            string TextM = "N/A";
            if(PencilButton.IsChecked == true){
                TextM = "This will replace all blocks inside the selection. Continue?";
            }
            else TextM = "This will replace ALL blocks in the map with the new Id. Are you sure you want to continue?";

            var ConfirmR = new ConfirmChoice
            {
                Text = TextM
            };

            if (ConfirmR.ShowDialog() == false)
            {
                return;
            }
            if(ConfirmR.Confirmed == true){
                if(PencilButton.IsChecked == true) ChunkEditor.ReplaceAreaValue(ChunkValue.Value, LayerValue.Value, SelectedArea.TileIdBeg,SelectedArea.TileIdEnd,replaceBlockWindow.FirstId,replaceBlockWindow.SecondId);
                else ChunkEditor.ReplaceBlockValue(replaceBlockWindow.FirstId,replaceBlockWindow.SecondId);
                RefreshTiles(ChunkValue.Value, LayerValue.Value);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    //For some reason Turtle's flattener doesn't work for me... And I need it for testing. 
    private void Flattener_OnClick(Object sender, RoutedEventArgs e){
        try
        {
            var ConfirmF = new ConfirmChoice
            {
                Text = "This will leave the map on bare bedrock.  Are you sure you want to continue?"
            };

            if (ConfirmF.ShowDialog() == false)
            {
                return;
            }

            ChunkEditor.Flatten();
            RefreshTiles(ChunkValue.Value, LayerValue.Value);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
    private void Gratitude_OnClick(Object sender, RoutedEventArgs e){
        try
        {
            var ValueEditor = new ValueEditor{
                Value = ChunkEditor.GratitudePoints,
                Text = "Gratitude Points [WARNING: MIGHT NOT WORK]",
                ImagePath = "/Images/Gratitude.png"
            };

            if (ValueEditor.ShowDialog() == false || !UInt32.TryParse(ValueEditor.ResponseText, out var value))
            {
                
                return;
            }
            ChunkEditor.GratitudePoints = value;
            ChunkEditor.UpdateExtra();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
    private void Clock_OnClick(Object sender, RoutedEventArgs e){
        try
        {
            var ValueEditor = new ValueEditor{
                Value = (uint)ChunkEditor.Clock,
                Text = "Time [From 0 to 1200]",
                ImagePath = "/Images/Clock.png"
            };

            if (ValueEditor.ShowDialog() == false || !float.TryParse(ValueEditor.ResponseText, out var value))
            {
                return;
            }
            if (value >= 0 && value <= 1200){
                ChunkEditor.Clock = value;
                ChunkEditor.UpdateExtra();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
    private void Weather_OnClick(Object sender, RoutedEventArgs e){
        try
        {
            var ChangeWeather = new ChangeWeather{
                Id = ChunkEditor.Weather
            };

            if (ChangeWeather.ShowDialog() == false)
            {
                return;
            }
            ChunkEditor.Weather = ChangeWeather.Id;
            ChunkEditor.UpdateExtra();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private void Size_OnClick(Object sender, RoutedEventArgs e){
        try
        {
            var ValueEditor = new ValueEditor{
                Value = (uint)ChunkEditor.ChunkCount,
                Text = "Chunk Count [TEMPORAL FIX, ONLY CHANGE THIS IF YOU KNOW THE VALUE IS WRONG. 'REPLACE ALL' AND 'FLATTENER' WILL CORRUPT THE SAVE IF THE CHUNK COUNT IS BIGGER THAN THE REAL VALUE]",
                ImagePath = ""
            };

            if (ValueEditor.ShowDialog() == false || !short.TryParse(ValueEditor.ResponseText, out var value))
            {
                
                return;
            }
            if (value >= 1 && value <= 800){
                ChunkEditor.ChunkCount = value;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    //PRIMITIVE FILTERING
    private void FilterByName(string TextKey)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            };

            var json = File.ReadAllText(@"Data\Tiles.json");

            var tiles = JsonSerializer.Deserialize<TileList>(json, options);

            TileComboBoxList.Clear();

            for (var i = 0; i < tiles!.Tiles.Count; i++)
            {
                if( ((tiles.Tiles[i].Name).ToLower()).Contains(TextKey)){
                    TileComboBoxList.Add(new ComboBoxTile
                    {
                        Id = i,
                        Tile = tiles.Tiles[i]
                    });
                    TileList.Add(tiles.Tiles[i]);
                }
                
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
        private void Search_OnClick(Object sender, RoutedEventArgs e){
        try
        {
            var ValueEditor = new ValueEditor{
                Value = 0,
                Text = "[PLACEHOLDER] Write name of block (or part of the name)",
                ImagePath = "/Images/Search.png"
            };

            if (ValueEditor.ShowDialog() == false )
            {
                
                return;
            }
            FilterByName((ValueEditor.ResponseText).ToLower());
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

}