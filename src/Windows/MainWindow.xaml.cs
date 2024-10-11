using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Policy;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DQB2ChunkEditor.Controls;
using DQB2ChunkEditor.Models;
using Microsoft.Win32;

namespace DQB2ChunkEditor.Windows;

public partial class MainWindow : Window
{
    public List<Tile> TileList { get; } = new();
    public ObservableProperty<Tile> SelectedTile { get; set; } = new();
    public ObservableProperty<bool> BuilderPlaced { get; set; } = new();
    public ObservableProperty<ushort> TrueId { get; set; } = new();
    public ObservableProperty<LayerTile> SelectedLayerTile { get; set; } = new();
    public ObservableProperty<short> ChunkValue { get; set; } = new() { Value = 0};
    public ObservableProperty<byte> LayerValue { get; set; } = new() { Value = 0 };
    public List<Weather> WeatherList { get; set; } = new();
    public ObservableCollection<ComboBoxWeather> WeatherComboBox { get; set; } = new();
    public List<ObservableProperty<SignText>> SignTextList { get; set; } = new List<ObservableProperty<SignText>>();

    public ObservableProperty<SelectionPencilClass> Test { get; set; } = new() { Value = null};

    public bool paint = false;
    public CanvasHandler CanvasH { get; set; } = new ();
    private void MouseDownCheck(object sender, MouseEventArgs e)
    {
        if (paint) { paint = false; Mouse.OverrideCursor = null; }
        }
    public MainWindow()
    {
        InitializeComponent();
        CreateDefaultTiles();
        CreateComboBoxTilesWeather();
        CreateMenuList();
        CanvasH.GridProcessing(GridMinimap, 1);
        CanvasH.Test.Value = CanvasH.SelectedArea;
        CanvasH.SelectionCanvas = SelectionCanvas;
        CanvasH.BGCanvas = BGCanvas;
        DataContext = this;
        ToolBar.ReplaceBlocksCommand += ReplaceBlocks;
        ToolBar.AreaRemovalCommand += AreaReset;
        ToolBar.PrintOutCommand += SaveData_OnClick;
        ToolBar.menuList = SelectionList;
        this.MouseDown += MouseDownCheck;
        this.SizeChanged += OnWindowSizeChanged;
        SelectionList.ReturnSelectedTile += SelectionTile_OnClick;
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
                layerTile.TileButton.MouseEnter += (_, _) => { LayerTile_OnDrag(layerTile); };

                LayerTiles.Children.Add(layerTile);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
    /// <summary>
    /// Creates the dropdown selection for changing weather
    /// </summary>
    private void CreateComboBoxTilesWeather()
    {
        try
        {
            var WeatherOptions = new JsonSerializerOptions
            {
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            };

            var Weatherjson = File.ReadAllText(@"Data\Weather.json");

            var weathers = JsonSerializer.Deserialize<WeatherList>(Weatherjson, WeatherOptions);

            for (var i = 0; i < weathers!.Weathers.Count; i++)
            {
                WeatherComboBox.Add(new ComboBoxWeather
                {
                    Id = i,
                    Weather = weathers.Weathers[i]
                });

                WeatherList.Add(weathers.Weathers[i]);
            }
            WeatherComboBoxX.SelectedIndex = ChunkEditor.Weather;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    /// <summary>
    /// Creates the dropdown selection tiles for changing a selected tile
    /// </summary>
    private void CreateMenuList()
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

            List<Tile> UsedBlocks = new List<Tile>();
            List<Tile> UnusedBlocks = new List<Tile>();
            List<Tile> NULLBlocks = new List<Tile>();
            List<Tile> IndestructibleBlocks = new List<Tile>();

            List<Tile> UsedLiquid = new List<Tile>();
            List<Tile> UnusedLiquid = new List<Tile>();
            List<Tile> NULLLiquid = new List<Tile>();

            foreach (var Tile in tiles.Tiles)
            {
                if(Tile.Name.Contains("Default Block"))
                {
                    NULLBlocks.Add(Tile);
                }else if (IsLiquid(Tile))
                {
                    if (Tile.Used == "Unused")
                    {
                        UnusedLiquid.Add(Tile);
                    }
                    else
                    {
                        UsedLiquid.Add(Tile);
                    }
                }
                else if(Tile.Used == "Unused")
                {
                    UnusedBlocks.Add(Tile);
                }
                else if (Tile.Break.Contains("Indestructible") || Tile.Break.Contains("Intangible"))
                {
                    IndestructibleBlocks.Add(Tile);
                }
                else
                {
                    UsedBlocks.Add(Tile);
                }
                TileList.Add(Tile);
            }
            var Temp = new List<List<Tile>>();
            Temp.Add(UsedBlocks);
            Temp.Add(UnusedBlocks);
            Temp.Add(IndestructibleBlocks);
            Temp.Add(NULLBlocks);
            SelectionList.BlockList = Temp;
            Temp = new List<List<Tile>>();
            Temp.Add(UsedLiquid);
            Temp.Add(UnusedLiquid);
            Temp.Add(NULLLiquid);
            SelectionList.LiquidList = Temp;
            Temp = new List<List<Tile>>();
            SelectionList.ObjectList = Temp;
            SelectionList.createTabList();
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

                ((LayerTile)LayerTiles.Children[i]).Tile.Value = TileList.FirstOrDefault(t => t.Id == blockId % 2048) ?? TileList[0];
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
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
            //if (SelectButton.IsChecked == true)
            switch (ToolBar.tool)
            {
                case 0:
                    var blockId = ChunkEditor.GetBlockValue(ChunkValue.Value, LayerValue.Value, SelectedLayerTile.Value.Id);
                    if (blockId > 2047){ BuilderPlaced.Value = true; }
                    else{ BuilderPlaced.Value = false; }
                    TrueId.Value = blockId;
                    SelectedTile.Value = layerTile.Tile.Value;
                    SelectionList.FavouriteList.SelectedToList(layerTile.Tile,false);
                    break;
                case 1:
                    if (paint) { paint = false; Mouse.OverrideCursor = null; }
                    else { paint = true; Mouse.OverrideCursor = ToolBar.paintCursor; }
                    if (!CanvasH.SelectedArea.HasArea || (SelectedLayerTile.Value!.Id >= CanvasH.SelectedArea.TileIdBeg && SelectedLayerTile.Value!.Id <= CanvasH.SelectedArea.TileIdBeg))
                    {
                        ((LayerTile)LayerTiles.Children[SelectedLayerTile.Value!.Id]).Tile.Value = SelectedTile.Value;
                        ChunkEditor.SetBlockValue(ChunkValue.Value, LayerValue.Value, SelectedLayerTile.Value.Id, (short)TrueId.Value);
                    }
                    break;
                case 2:
                    var coordLine = ((LayerTile)LayerTiles.Children[SelectedLayerTile.Value!.Id]).ActualHeight;
                    var x0 = (SelectedLayerTile.Value.Id % 32) * coordLine;
                    var y0 = (SelectedLayerTile.Value.Id / 32) * coordLine;
                    CanvasH.LineHandler(x0, y0, coordLine, SelectedLayerTile.Value!.Id, LayerValue.Value);
                    Test.Value = CanvasH.Test.Value;
                    break;
                case 3:
                    var blockId2 = ChunkEditor.GetBlockValue(ChunkValue.Value, LayerValue.Value, SelectedLayerTile.Value.Id);
                    if (blockId2 > 2047) { BuilderPlaced.Value = true; }
                    else { BuilderPlaced.Value = false; }
                    TrueId.Value = blockId2;
                    SelectedTile.Value = layerTile.Tile.Value;
                    SelectionList.FavouriteList.SelectedToList(layerTile.Tile, false);
                    ToolBar.AddToList(layerTile.Tile);
                    break;
                default:
                    break;

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
    private void LayerTile_OnDrag(LayerTile layerTile)
    {
        try
        {
            if (!paint || ToolBar.tool != 1) { paint = false; return; }
            if (layerTile.Tile.Value == null || string.IsNullOrWhiteSpace(ChunkEditor.Filename))
            {
                return;
            }

            SelectedLayerTile.Value = layerTile;

            // if the select button is check, update the dropdown
            //if (SelectButton.IsChecked == true)
            switch (ToolBar.tool)
            {
                case 0:
                    break;
                case 1:
                    if (!CanvasH.SelectedArea.HasArea || (SelectedLayerTile.Value!.Id >= CanvasH.SelectedArea.TileIdBeg && SelectedLayerTile.Value!.Id <= CanvasH.SelectedArea.TileIdBeg))
                    {
                        ((LayerTile)LayerTiles.Children[SelectedLayerTile.Value!.Id]).Tile.Value = SelectedTile.Value;
                        ChunkEditor.SetBlockValue(ChunkValue.Value, LayerValue.Value, SelectedLayerTile.Value.Id, (short)TrueId.Value);
                    }
                    break;
                default:
                    break;

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
    private void SelectionTile_OnClick(ObservableProperty<Tile> NewSelectedTile)
    {
        BuilderPlaced.Value = false;
        TrueId.Value = (ushort)NewSelectedTile.Value.Id;
        SelectedTile.Value = NewSelectedTile.Value;
        if (ToolBar.tool == 3)
        {
            ToolBar.AddToList(NewSelectedTile);
        }
        else
        {
            if (CanvasH.SelectedArea.HasArea)
            {
                ChunkEditor.ReplaceBlockValue(ChunkValue.Value, ChunkValue.Value,
                                               CanvasH.SelectedArea.LayerBeggining, CanvasH.SelectedArea.LayerEnd,
                                               CanvasH.SelectedArea.TileIdBeg, CanvasH.SelectedArea.TileIdEnd,
                                               null, (short)TrueId.Value);
                RefreshTiles(ChunkValue.Value, LayerValue.Value);
            }
        }
    }

    public void ReplaceBlocks(List<BlockTileSquare> BlockList, short newId)
    {
        List<short> IdList = new List<short>();
        foreach(var Id in BlockList)
        {
            IdList.Add(Id.Tile.Value.Id);
        }
        if (CanvasH.SelectedArea.HasArea)
        {
            ChunkEditor.ReplaceBlockValue(ChunkValue.Value, ChunkValue.Value,
                                           CanvasH.SelectedArea.LayerBeggining, CanvasH.SelectedArea.LayerEnd,
                                           CanvasH.SelectedArea.TileIdBeg, CanvasH.SelectedArea.TileIdEnd,
                                           IdList, newId);
            RefreshTiles(ChunkValue.Value, LayerValue.Value);
        }
        else
        {
            ChunkEditor.ReplaceBlockValue(0, ChunkEditor.ChunkCount,
                               0, (short)ChunkEditor.LayerHeight,
                               0, 1024,
                               IdList, newId);
            RefreshTiles(ChunkValue.Value, LayerValue.Value);
        }
    }
    private void AreaReset()
    {
        CanvasH.SelectedArea = new();
        CanvasH.Test.Value = CanvasH.SelectedArea;
        CanvasH.BGCanvas.Children.Clear();
        CanvasH.SelectionCanvas.Children.Clear();
        Test.Value = CanvasH.Test.Value;
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
            if (ChunkValue.Value >= ChunkEditor.ChunkCount)
            {
                ChunkValue.Value = ChunkEditor.ChunkCount;
            }

            RefreshTiles(ChunkValue.Value, LayerValue.Value);
            ReadSign();
            GratitudeInput.Text = Convert.ToString(ChunkEditor.GratitudePoints);
            TimeInput.Text = Convert.ToString(ChunkEditor.Clock);
            WeatherComboBoxX.SelectedIndex = ChunkEditor.Weather;
            LayerTiles.Visibility = Visibility.Visible;
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
    private async void SaveFile_OnClick(object sender, RoutedEventArgs e)
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
                FileName = "STGDAT" + ChunkEditor.Island.ToString("D2")
            };
            if (saveFileDialog.ShowDialog() == false)
            {
                return;
            }
            Saving.Visibility = Visibility.Visible;
            MainGrid.IsEnabled = false;
            ChunkEditor.Filename = saveFileDialog.FileName;
            ChunkEditor.SaveSign(SignTextList);
            await Task.Run(() => ChunkEditor.SaveFile());
            MainGrid.IsEnabled = true;
            Saving.Visibility = Visibility.Collapsed;
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
                FileName = "STGDAT" + ChunkEditor.Island.ToString("D2")
            };

            if (saveFileDialog.ShowDialog() == false)
            {
                return;
            }

            ChunkEditor.SaveSign(SignTextList);
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
            if (ChunkValue.Value >= ChunkEditor.ChunkCount)
            {
                ChunkValue.Value = ChunkEditor.ChunkCount;
            }

            RefreshTiles(ChunkValue.Value, LayerValue.Value);
            ReadSign();
            GratitudeInput.Text = Convert.ToString(ChunkEditor.GratitudePoints);
            TimeInput.Text = Convert.ToString(ChunkEditor.Clock);
            WeatherComboBoxX.SelectedIndex = ChunkEditor.Weather;
            LayerTiles.Visibility = Visibility.Visible;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            MessageBox.Show(ex.Message, "Failed to import file", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ReadSign()
    {
        SignTextList = ChunkEditor.LoadSign();
        List.ItemsSource = SignTextList;

    }
    private void SaveData_OnClick(bool isObject)
    {
        try
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Text file (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if (saveFileDialog.ShowDialog() == false)
            {
                return;
            }
            if (isObject)
            {
                var ListI = ChunkEditor.CountItemData();
                if (ListI == null) return;
                using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                {
                    for (int i = 0; i < ListI.Length; i++)
                    {
                        if (ListI[i] > 0)
                        {
                            writer.WriteLine(ListI[i] + "\tItemID" + i);
                        }
                    }
                }
            }
            else
            {
                var List = ChunkEditor.CountBlockData();
                if (List == null) return;
                using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                {
                    for (int i = 0; i < 2048; i++)
                    {
                        writer.WriteLine(List[i] + "\t" + i + "\t" + TileList[i + 1].Name);
                    }
                }
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    /// <summary>
    /// Change the selected chunk area. Updates the layer tiles.
    /// </summary>
    //private void ChunkInput_OnMouseUp(Object sender, MouseButtonEventArgs e)
    //{
    //    try
    //    {
    //        var inputValueDialog = new InputValue
    //        {
    //            MaxValue = ChunkEditor.ChunkCount - 1,
    //            CurrentValue = ChunkValue.Value
    //        };

    //        if (inputValueDialog.ShowDialog() == false ||
    //            !short.TryParse(inputValueDialog.ResponseText, out var value))
    //        {
    //            return;
    //        }

    //        TrySetChunk(value);
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine(ex);
    //    }
    //}

    ///// <summary>
    ///// Change the selected layer for a chunk. Updates the layer tiles.
    ///// </summary>
    //private void LayerInput_OnMouseUp(Object sender, MouseButtonEventArgs e)
    //{
    //    try
    //    {
    //        var inputValueDialog = new InputValue
    //        {
    //            CurrentValue = LayerValue.Value
    //        };

    //        if (inputValueDialog.ShowDialog() == false ||
    //            !byte.TryParse(inputValueDialog.ResponseText, out var value))
    //        {
    //            return;
    //        }

    //        TrySetLayer(value);
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine(ex);
    //    }
    //}

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
    private void TrySetChunk(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var box = sender as TextBox;
            if (!Int32.TryParse(box.Text, out int numValue) || numValue < 0 || numValue >= ChunkEditor.ChunkCount)
            {
                return;
            }

            ChunkValue.Value = (byte)numValue;

            RefreshTiles(ChunkValue.Value, LayerValue.Value);
        }
    }

    private void TrySetLayer(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var box = sender as TextBox;
            if (!Int32.TryParse(box.Text, out int numValue) || numValue is < 0 or > 95)
            {
                return;
            }

            LayerValue.Value = (byte)numValue;

            RefreshTiles(ChunkValue.Value, LayerValue.Value);
            CanvasH.UpdateLayer(LayerValue.Value);
            Test.Value = CanvasH.Test.Value;
        }
    }
    private void TrySetLayer(byte Value)
    {
            if (Value is < 0 or > 95)
            {
                return;
            }

            LayerValue.Value = Value;

            RefreshTiles(ChunkValue.Value, LayerValue.Value);
        CanvasH.UpdateLayer(LayerValue.Value);
        Test.Value = CanvasH.Test.Value;
    }

    private bool IsLiquid(Tile Tile)
    {
        if (Tile != null)
        {
            if ((Tile.Name.ToLower().Contains("water") || Tile.Name.Contains("Plasma") || Tile.Name.Contains("Bottomless") || Tile.Name.Contains("Poison ") || Tile.Name.Contains("Lava "))
                && Tile.Id != 593 && Tile.Id != 17 && Tile.Id != 86) { return true; }
            else { return false; }
        }
        return false;
    }
    protected void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
    {
        SelectionList.ScrollViewUpdate(SelectionList.ActualHeight);
        PreviewText.Height = e.NewSize.Width/2;
        SelectionList.BlockMenu.TextBoxFilter.Width = SelectionList.ActualWidth - 10;
        SelectionList.LiquidMenu.TextBoxFilter.Width = SelectionList.ActualWidth - 10;
        //SelectionList.ObjectMenu.TextBoxFilter.Width = SelectionList.ActualWidth - 10;
    }

    private void TimeInput_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            if (!float.TryParse(TimeInput.Text, out var value) || value > 1200 || value < 0)
            {
                return;
            }
            else
            {
                ChunkEditor.Clock = value;
                if (ChunkEditor.Island != 0xFF)
                {
                    ChunkEditor.UpdateExtra();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private void SetWeather_OnClick(object sender, SelectionChangedEventArgs e)
    {
        ChunkEditor.Weather = ((ComboBoxWeather)WeatherComboBoxX.SelectedItem).Weather.Id;
        if (ChunkEditor.Island != 0xFF)
        {
            ChunkEditor.UpdateExtra();
        }
    }
    public void CommandPreview(object sender, EventArgs e)
    {
        string Line = (sender as Button).Tag.ToString();
        if (Line != null)
        {
            var elementsToRemove = Pain.Children.OfType<UserControl>()
                                                  .Where(e => e.GetType() == typeof(Preview))
                                                  .ToList();  // Create a list to avoid collection modification issues

            // Remove each element from the Grid
            foreach (var element in elementsToRemove)
            {
                Pain.Children.Remove(element);
            }
            var TextBox = new Preview();
            TextBox.LoadPreview(Line);
            Grid.SetColumn(TextBox, 1);
            Grid.SetRow(TextBox, 1);
            Pain.Children.Add(TextBox);
        }
    }
    
}
