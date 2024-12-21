using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
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
using System.Windows.Shapes;
using DQB2ChunkEditor.Controls;
using DQB2ChunkEditor.Models;
using Microsoft.Win32;
using static System.Net.WebRequestMethods;

namespace DQB2ChunkEditor.Windows;

public partial class MainWindow : Window
{
    public List<TileData> TileList { get; } = new();
    public List<TileData> TileListObject { get; } = new();

    private List<ItemInstance> CurrentObjectsList;
    public ObservableProperty<Tile> SelectedTile { get; set; } = new();
    public ObservableProperty<TileDataExtra> ExtrasTileData { get; set; } = new();
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
        ChunkGrid.ChunkClick += TrySetChunk;
        this.MouseDown += MouseDownCheck;
        this.SizeChanged += OnWindowSizeChanged;
        SelectionList.ReturnSelectedTile += SelectionTile_OnClick;
        YLayerControl.Layer += TrySetLayer;
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

            var Weatherjson = System.IO.File.ReadAllText(@"Data\Weather.json");

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
            var Extra = new TileDataExtra(0);

            List<TileData> UsedBlocks = new List<TileData>();
            List<TileData> UnusedBlocks = new List<TileData>();
            List<TileData> NULLBlocks = new List<TileData>();
            List<TileData> IndestructibleBlocks = new List<TileData>();

            List<TileData> UsedLiquid = new List<TileData>();
            List<TileData> UnusedLiquid = new List<TileData>();
            List<TileData> NULLLiquid = new List<TileData>();

            IEnumerable<string> BlockFile = System.IO.File.ReadLines("Data/Blocks.txt");

            foreach(var Line in BlockFile)
            {
                if (Line[0] == '#') continue;
                String[] values = Line.Split('\t');
                var Tile = new TileData(short.Parse(values[0]), short.Parse(values[1]), values[2] == "1", short.Parse(values[4]),values[5],null);
                switch (values[3][0]){
                    case '0':
                        if (values[2][0] == '1') UsedLiquid.Add(Tile);
                        else UsedBlocks.Add(Tile);
                        break;
                    case '1':
                        if (values[2][0] == '1') UnusedLiquid.Add(Tile);
                        else UnusedBlocks.Add(Tile);
                        break;
                    case '2':
                        if (values[2][0] == '1') NULLLiquid.Add(Tile);
                        else NULLBlocks.Add(Tile);
                        break;
                    case '3':
                        IndestructibleBlocks.Add(Tile);
                        break;
                }
                TileList.Add(Tile);
            }
            var Temp = new List<List<TileData>>();
            Temp.Add(UsedBlocks);
            Temp.Add(UnusedBlocks);
            Temp.Add(IndestructibleBlocks);
            Temp.Add(NULLBlocks);
            SelectionList.BlockList = Temp;
            Temp = new List<List<TileData>>();
            Temp.Add(UsedLiquid);
            Temp.Add(UnusedLiquid);
            Temp.Add(NULLLiquid);
            SelectionList.LiquidList = Temp;

            List<TileData> UsedObject = new List<TileData>();
            List<TileData> UnusedObject = new List<TileData>();
            List<TileData> NULLObject = new List<TileData>();

            IEnumerable<string> ItemFile = System.IO.File.ReadLines("Data/Items.txt");

            foreach (var Line in ItemFile)
            {
                if (Line[0] == '#') continue;
                String[] values = Line.Split('\t');
                var Tile = new TileData(short.Parse(values[0]), short.Parse(values[1]), false, 0, values[2], new TileItemData(1,1,1));
                UsedObject.Add(Tile);
                TileListObject.Add(Tile);
            }

            Temp = new List<List<TileData>>();
            Temp.Add(UsedObject);
            Temp.Add(UnusedObject);
            Temp.Add(NULLObject);
            SelectionList.ObjectList = Temp;

            SelectionList.createTabList();
            ChunkEditor.ObjectList = TileListObject;
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
            CurrentObjectsList = ChunkEditor.ReadItemsChunkLayer(chunk, layer);
            for (short i = 0; i < 1024; i++)
            {
                var blockId = ChunkEditor.GetBlockValue(chunk, layer, i);
                ((LayerTile)LayerTiles.Children[i]).Tile.Value = new Tile(TileList.FirstOrDefault(t => t.Id == blockId % 2048) ?? TileList[0])
                {
                    Id = blockId
                }; 
                ((LayerTile)LayerTiles.Children[i]).Tile.Value.seeThrough = false;
            }
            foreach (var item in CurrentObjectsList)
            {
                var VChunk = ChunkEditor.GetGridFromChunk(chunk);
                //item.Id = (short)((LayerTile)LayerTiles.Children[item.PosX + (item.PosZ * 32)]).Tile.Value.Id;
                if (item.ChunkGrid != VChunk)
                {
                    if(item.ChunkGrid - 1 == VChunk)
                    {
                        foreach (var Z in item.Zcoords(false))
                        {
                            foreach (var X in item.Xcoords(true))
                            {
                                ((LayerTile)LayerTiles.Children[X + (Z * 32)]).Tile.Value.ItemInstance = item;
                                ((LayerTile)LayerTiles.Children[X + (Z * 32)]).Tile.Value.seeThrough = true;
                            }
                        }
                    }
                    else
                    {
                        if(item.ChunkGrid == VChunk - 64)
                        {
                            foreach (var Z in item.Zcoords(true))
                            {
                                foreach (var X in item.Xcoords(false))
                                {
                                    ((LayerTile)LayerTiles.Children[X + (Z * 32)]).Tile.Value.ItemInstance = item;
                                    ((LayerTile)LayerTiles.Children[X + (Z * 32)]).Tile.Value.seeThrough = true;
                                }
                            }
                        }
                        else
                        {
                            foreach (var Z in item.Zcoords(true))
                            {
                                foreach (var X in item.Xcoords(true))
                                {
                                    ((LayerTile)LayerTiles.Children[X + (Z * 32)]).Tile.Value.ItemInstance = item;
                                    ((LayerTile)LayerTiles.Children[X + (Z * 32)]).Tile.Value.seeThrough = true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (var Z in item.Zcoords(false))
                    {
                        foreach (var X in item.Xcoords(false))
                        {
                            ((LayerTile)LayerTiles.Children[X + (Z * 32)]).Tile.Value.ItemInstance = item;
                            ((LayerTile)LayerTiles.Children[X + (Z * 32)]).Tile.Value.seeThrough = true;
                        }
                    }
                    if (item.PosY == layer)
                    {
                        ((LayerTile)LayerTiles.Children[item.PosX + (item.PosZ * 32)]).Tile.Value.ItemInstance = item;
                        ((LayerTile)LayerTiles.Children[item.PosX + (item.PosZ * 32)]).Tile.Value.seeThrough = false;
                    }
                }
                ((LayerTile)LayerTiles.Children[item.PosX + (item.PosZ * 32)]).Tile.NotifyValue();


            }
            CanvasH.UpdateCanvas(layer, (ushort)ChunkEditor.GetGridFromChunk(chunk));

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
                    SelectedTile.Value = layerTile.Tile.Value;
                    ExtrasTileData.Value = new TileDataExtra(SelectedTile.Value.TileData.Id, SelectedTile.Value.TileData.isObject);
                    SelectionList.FavouriteList.SelectedToList(layerTile.Tile.Value.TileData, false);
                    break;
                case 1:
                    if (SelectedTile.Value.TileData.isObject == true)
                    {
                        paint = false; Mouse.OverrideCursor = null;
                        //((LayerTile)LayerTiles.Children[layerTile!.Id]).Tile.Value = SelectedTile.Value;
                        //ChunkEditor.SetItemValue(ChunkValue.Value, LayerValue.Value, layerTile.Id, SelectedTile.Value);
                    }
                    else
                    {
                        if (paint) { paint = false; Mouse.OverrideCursor = null; }
                        else { paint = true; Mouse.OverrideCursor = ToolBar.paintCursor; }
                    }
                    break;
                case 2:
                    CanvasH.LineHandler((ushort)(SelectedLayerTile.Value!.Id), LayerValue.Value, (ushort)ChunkEditor.GetGridFromChunk(ChunkValue.Value));
                    ChunkGrid.UpdateSelection(CanvasH.SelectedArea.HasArea,CanvasH.SelectedArea.VirtualChunkBeggining, CanvasH.SelectedArea.VirtualChunkEnd,
                        CanvasH.SelectedArea.x0, CanvasH.SelectedArea.y0, CanvasH.SelectedArea.x1, CanvasH.SelectedArea.y1);
                    Test.Value = CanvasH.Test.Value;
                    break;
                case 3:
                    SelectedTile.Value = layerTile.Tile.Value;
                    ExtrasTileData.Value = new TileDataExtra(SelectedTile.Value.TileData.Id, SelectedTile.Value.TileData.isObject);
                    SelectionList.FavouriteList.SelectedToList(layerTile.Tile.Value.TileData, false);
                    ToolBar.AddToList(layerTile.Tile.Value.TileData);
                    break;
                case 4:
                    if (layerTile.Tile.Value.TileData.isObject && ToolBar.Grab.IsChecked == true)
                    {
                        SelectedTile.Value = layerTile.Tile.Value;
                        ExtrasTileData.Value = new TileDataExtra(SelectedTile.Value.TileData.Id, SelectedTile.Value.TileData.isObject);
                        ToolBar.GlovesList.AddToList(layerTile.Tile.Value.ItemInstance);
                    }else
                    if (ToolBar.Place.IsChecked == true)
                    {
                        //foreach (var item in CurrentObjectsList)
                        //{
                        //    if (item.PosX + item.PosZ * 32 == layerTile!.Id)
                        //    {
                        //        ChunkEditor.DeleteItem(item);
                        //        CurrentObjectsList.Remove(item);
                        //    }
                        //}
                        //ChunkEditor.SetItem(SelectedTile.ItemInstance.Value, ChunkValue.Value, LayerValue.Value,layerTile.Id);
                        //RefreshTiles(ChunkValue.Value, LayerValue.Value);
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
                    if (SelectedTile.Value.TileData.isObject == true)
                    {
                        paint = false; Mouse.OverrideCursor = null;
                        return;
                    }
                    if (CanvasH.InsideArea(layerTile!.Id))
                    {
                        if (layerTile.Tile.Value.TileData.isObject)
                        {
                            foreach (var item in CurrentObjectsList)
                            {
                                if (item.PosX + item.PosZ * 32 == layerTile!.Id)
                                {
                                    ChunkEditor.DeleteItem(item);
                                    CurrentObjectsList.Remove(item);
                                }
                            }
                        }
                        ((LayerTile)LayerTiles.Children[layerTile!.Id]).Tile.Value = SelectedTile.Value;
                        ChunkEditor.SetBlockValue(ChunkValue.Value, LayerValue.Value, layerTile.Id, (short)layerTile.Tile.Value.Id);
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
    private void SelectionTile_OnClick(TileData NewSelectedTile)
    {
        SelectedTile.Value = new Tile(NewSelectedTile)
        {
            Id = (ushort)NewSelectedTile.Id
        };
        ExtrasTileData.Value = new TileDataExtra(SelectedTile.Value.TileData.Id, SelectedTile.Value.TileData.isObject);
        //GOTTA DO ITEM INIT. STUFF. AND OVERFLOW.
        if (ToolBar.tool == 3)
        {
            ToolBar.AddToList(NewSelectedTile);
        }
        else
        {
            if (CanvasH.SelectedArea.HasArea)
            {
                ChunkEditor.ReplaceBlockValueNew(CanvasH.SelectedArea.VirtualChunkBeggining, CanvasH.SelectedArea.VirtualChunkEnd,
                                               (ushort)CanvasH.SelectedArea.LayerBeggining, (ushort)CanvasH.SelectedArea.LayerEnd,
                                               CanvasH.SelectedArea.x0, CanvasH.SelectedArea.y0, CanvasH.SelectedArea.x1, CanvasH.SelectedArea.y1,
                                               null, (short)SelectedTile.Value.Id);
                foreach (var item in CurrentObjectsList)
                {
                    if (CanvasH.InsideArea((short)(item.PosX + item.PosZ * 32)))
                    {
                        ChunkEditor.DeleteItem(item);
                    }
                }
                RefreshTiles(ChunkValue.Value, LayerValue.Value);
            }
        }
    }

    private void SelectionGloves_OnClick(ItemInstance NewItem)
    {
        if (ToolBar.tool == 4)
        {
        }
        else
        {
           
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
            ChunkEditor.ReplaceBlockValueNew(CanvasH.SelectedArea.VirtualChunkBeggining, CanvasH.SelectedArea.VirtualChunkEnd,
                                                           (ushort)CanvasH.SelectedArea.LayerBeggining, (ushort)CanvasH.SelectedArea.LayerEnd,
                                                           CanvasH.SelectedArea.x0, CanvasH.SelectedArea.y0, CanvasH.SelectedArea.x1, CanvasH.SelectedArea.y1,
                                                            IdList, newId);
            RefreshTiles(ChunkValue.Value, LayerValue.Value);
        }
        else
        {
            ChunkEditor.ReplaceBlockValueNew(0, 64*64,
                               0, (ushort)ChunkEditor.LayerHeight,
                               0, 0,32,32,
                               IdList, newId);
            RefreshTiles(ChunkValue.Value, LayerValue.Value);
        }
    }
    private void AreaReset(bool th)
    {
        if (th)
            CanvasH.SelectedArea.TileIdEnd = -1;
        else
            CanvasH.SelectedArea.TileIdBeg = -1;
        CanvasH.Test.Value = CanvasH.SelectedArea;
        CanvasH.UpdateCanvas(LayerValue.Value, (ushort)ChunkEditor.GetGridFromChunk(ChunkValue.Value));
        Test.Value = CanvasH.Test.Value;
        ChunkGrid.UpdateSelection(CanvasH.SelectedArea.HasArea, CanvasH.SelectedArea.VirtualChunkBeggining, CanvasH.SelectedArea.VirtualChunkEnd,
                        CanvasH.SelectedArea.x0, CanvasH.SelectedArea.y0, CanvasH.SelectedArea.x1, CanvasH.SelectedArea.y1);
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
            YLayerControl.UpdateSeaLevel();

            // if we are above the max chunk, reset to the highest chunk
            if (ChunkValue.Value >= ChunkEditor.VirtualChunkCount)
            {
                ChunkValue.Value = ChunkEditor.VirtualChunkCount;
            }

            RefreshTiles(ChunkValue.Value, LayerValue.Value);
            ReadSign();
            AreaReset(false);
            AreaReset(true);
            GratitudeInput.Text = Convert.ToString(ChunkEditor.GratitudePoints);
            TimeInput.Text = Convert.ToString(ChunkEditor.Clock);
            WeatherComboBoxX.SelectedIndex = ChunkEditor.Weather;
            LayerTiles.Visibility = Visibility.Visible;
            ChunkGrid.CreateMainGrid();
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
            YLayerControl.UpdateSeaLevel();

            // if we are above the max chunk, reset to the highest chunk
            if (ChunkValue.Value >= ChunkEditor.VirtualChunkCount)
            {
                ChunkValue.Value = ChunkEditor.VirtualChunkCount;
            }

            RefreshTiles(ChunkValue.Value, LayerValue.Value);
            ReadSign();
            AreaReset(false);
            AreaReset(true);
            GratitudeInput.Text = Convert.ToString(ChunkEditor.GratitudePoints);
            TimeInput.Text = Convert.ToString(ChunkEditor.Clock);
            WeatherComboBoxX.SelectedIndex = ChunkEditor.Weather;
            LayerTiles.Visibility = Visibility.Visible;
            ChunkGrid.CreateMainGrid();
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
            };

            if (saveFileDialog.ShowDialog() == false)
            {
                return;
            }
            if (isObject)
            {
                using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                {
                    ChunkEditor.CountItemData(writer,TileListObject);
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
                        writer.WriteLine(List[i] + "\t" + i + "\t" + TileList[i].Name);
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
    public void TrySetChunk(short value)
    {
        if (value < 0 || value >= ChunkEditor.VirtualChunkCount)
        {
            return;
        }

        ChunkValue.Value = value;
        Tabs.SelectedIndex = 0;
        RefreshTiles(ChunkValue.Value, LayerValue.Value);
    }
    private void TrySetChunk(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var box = sender as TextBox;
            if (!Int32.TryParse(box.Text, out int numValue) || numValue < 0 || numValue >= ChunkEditor.VirtualChunkCount)
            {
                return;
            }

            ChunkValue.Value = (byte)numValue;

            RefreshTiles(ChunkValue.Value, LayerValue.Value);
            CanvasH.coordLine = ((LayerTile)LayerTiles.Children[SelectedLayerTile.Value!.Id]).ActualHeight;
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
        YLayerControl.LayerChange(LayerValue.Value);
        Test.Value = CanvasH.Test.Value;
    }
    protected void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
    {
        try
        {
            CanvasH.coordLine = ((LayerTile)LayerTiles.Children[1]).ActualHeight;
            SelectionList.ScrollViewUpdate(SelectionList.ActualHeight);
            PreviewText.Height = e.NewSize.Width / 2;
            YLayerControl.YLayerLine.Height = YLayerControl.GridLine.Height;
            YLayerControl.ArrowLayer.Width = YLayerControl.YLayerLine.ActualWidth * 0.7;
            YLayerControl.LayerChange(LayerValue.Value); //Arrow shenanigans...
            SelectionList.BlockMenu.TextBoxFilter.Width = SelectionList.ActualWidth - 10;
            SelectionList.LiquidMenu.TextBoxFilter.Width = SelectionList.ActualWidth - 10;
            SelectionList.ObjectMenu.TextBoxFilter.Width = SelectionList.ActualWidth - 10;
        }
        catch { }
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

    private void ConfirmOnClick(object sender, RoutedEventArgs e)
    {
        if (byte.TryParse(LayerInput.Text, out byte var))
        {
            if (var != LayerValue.Value)
            {
                TrySetLayer(var);
                LayerValue.NotifyValue();
                LayerInput.Background = Brushes.White;
                ConfirmLayer.Background = Brushes.White;
            }
        }
        else
        {
            LayerValue.NotifyValue();
            LayerInput.Background = Brushes.White;
            ConfirmLayer.Background = Brushes.White;
        }
    }

    private void LayerTextChange(object sender, TextChangedEventArgs e)
    {
        if(byte.TryParse(LayerInput.Text,out byte var))
        {
            if(var != LayerValue.Value)
            {
                LayerInput.Background = Brushes.Orange;
                ConfirmLayer.Background = Brushes.Orange;
            }
            else
            {
                LayerInput.Background = Brushes.White;
                ConfirmLayer.Background = Brushes.White;
            }
        }
        else
        {
            LayerInput.Background = Brushes.Red;
            ConfirmLayer.Background = Brushes.White;
        }
    }

    private void VirtualChunkChange(object sender, RoutedEventArgs e)
    {
        Button button = (Button)sender;
        uint ChangeChunk = ChunkEditor.GetGridFromChunk(ChunkValue.Value);
        switch (button.Name){
            case "VChunkUp":
                ChangeChunk = ChangeChunk - 64;
                break;
            case "VChunkDown":
                ChangeChunk = ChangeChunk + 64;
                break;
            case "VChunkLeft":
                ChangeChunk = ChangeChunk - 1;
                break;
            case "VChunkRight":
                ChangeChunk = ChangeChunk + 1;
                break;
        }
        TrySetChunk((short)ChunkEditor.GetChunkFromGrid((int)ChangeChunk));
    }
}
