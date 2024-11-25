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
using DQBChunkEditor.Controls;
using DQBChunkEditor.Models;
using Microsoft.Win32;

namespace DQBChunkEditor.Windows;

public partial class MainWindow : Window
{
    public List<TileData> TileList { get; } = new();
    public List<TileData> TileListObject { get; } = new();

    private List<ItemInstance> CurrentObjectsList;
    public ObservableProperty<Tile> SelectedTile { get; set; } = new();
    public ObservableProperty<LayerTile> SelectedLayerTile { get; set; } = new();
    public ObservableProperty<short> ChunkValue { get; set; } = new() { Value = 0};
    public ObservableProperty<byte> LayerValue { get; set; } = new() { Value = 0 };
    public List<Weather> WeatherList { get; set; } = new();
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
    /// Creates the dropdown selection tiles for changing a selected tile
    /// </summary>
    private void CreateMenuList()
    {
        try
        {

            List<TileData> UsedBlocks = new List<TileData>();
            List<TileData> UnusedBlocks = new List<TileData>();
            List<TileData> NULLBlocks = new List<TileData>();
            List<TileData> IndestructibleBlocks = new List<TileData>();

            List<TileData> UsedLiquid = new List<TileData>();
            List<TileData> UnusedLiquid = new List<TileData>();
            List<TileData> NULLLiquid = new List<TileData>();

            for(short i = 0; i < 255; i++)
            {
                var Tile = new TileData(i);
                if (Tile.Name.Contains("Default Block"))
                {
                    NULLBlocks.Add(Tile);
                }else if (IsLiquid(Tile))
                {
                    if (Tile.Extras.Used == "Unused")
                    {
                        UnusedLiquid.Add(Tile);
                    }
                    else
                    {
                        UsedLiquid.Add(Tile);
                    }
                }
                else if(Tile.Extras.Used == "Unused")
                {
                    UnusedBlocks.Add(Tile);
                }
                else if (Tile.Extras.BrokenBy.Contains("Indestructible") || Tile.Extras.BrokenBy.Contains("Intangible"))
                {
                    IndestructibleBlocks.Add(Tile);
                }
                else
                {
                    UsedBlocks.Add(Tile);
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

            for (short i = 1; i < 255; i++)
            {
                var Tile = new TileData(i,true);
                TileListObject.Add(Tile);
                UsedObject.Add(Tile);
            }

            Temp = new List<List<TileData>>();
            Temp.Add(UsedObject);
            Temp.Add(UnusedObject);
            Temp.Add(NULLObject);
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
                ((LayerTile)LayerTiles.Children[i]).Tile.Value = new Tile(TileList.FirstOrDefault(t => t.Id == blockId % 2048) ?? TileList[0])
                {
                    Id = blockId
                };
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
                    SelectedTile.Value = layerTile.Tile.Value;
                    SelectionList.FavouriteList.SelectedToList(layerTile.Tile.Value.TileData, false);
                    break;
                case 1:
                    if (SelectedTile.Value.TileData.isObject)
                    {
                        paint = false; Mouse.OverrideCursor = null;
                        //((LayerTile)LayerTiles.Children[layerTile!.Id]).Tile.Value = SelectedTile.Value;
                        //ChunkEditor.SetItemValue(ChunkValue.Value, LayerValue.Value, layerTile.Id, SelectedTile.Value);
                    }
                    if (paint) { paint = false; Mouse.OverrideCursor = null; }
                    else { paint = true; Mouse.OverrideCursor = ToolBar.paintCursor; }
                    break;
                case 2:
                    var coordLine = ((LayerTile)LayerTiles.Children[SelectedLayerTile.Value!.Id]).ActualHeight;
                    var x0 = (SelectedLayerTile.Value.Id % 32) * coordLine;
                    var y0 = (SelectedLayerTile.Value.Id / 32) * coordLine;
                    CanvasH.LineHandler(x0, y0, coordLine, SelectedLayerTile.Value!.Id, LayerValue.Value);
                    Test.Value = CanvasH.Test.Value;
                    break;
                case 3:
                    SelectedTile.Value = layerTile.Tile.Value;
                    SelectionList.FavouriteList.SelectedToList(layerTile.Tile.Value.TileData, false);
                    ToolBar.AddToList(layerTile.Tile.Value.TileData);
                    break;
                case 4:
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
                    if (CanvasH.InsideArea(layerTile!.Id))
                    {
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
        //GOTTA DO ITEM INIT. STUFF. AND OVERFLOW.
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
                                               null, (short)SelectedTile.Value.Id);
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
                Filter = "WDAT*.BIN|WDAT*.BIN"
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
                FileName = "WDAT*"
            };
            if (saveFileDialog.ShowDialog() == false)
            {
                return;
            }
            Saving.Visibility = Visibility.Visible;
            MainGrid.IsEnabled = false;
            ChunkEditor.Filename = saveFileDialog.FileName;
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
                FileName = "WDAT*"
            };

            if (saveFileDialog.ShowDialog() == false)
            {
                return;
            }


            //ChunkEditor.ExportFile(saveFileDialog.FileName);
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

            //ChunkEditor.ImportFile(importFileDialog.FileName);

            // if we are above the max chunk, reset to the highest chunk
            if (ChunkValue.Value >= ChunkEditor.ChunkCount)
            {
                ChunkValue.Value = ChunkEditor.ChunkCount;
            }

            RefreshTiles(ChunkValue.Value, LayerValue.Value);
            LayerTiles.Visibility = Visibility.Visible;
            ChunkGrid.CreateMainGrid();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            MessageBox.Show(ex.Message, "Failed to import file", MessageBoxButton.OK, MessageBoxImage.Error);
        }
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
        if (value < 0 || value >= ChunkEditor.ChunkCount)
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

    private bool IsLiquid(TileData Tile)
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
        try
        {
            SelectionList.ScrollViewUpdate(SelectionList.ActualHeight);
            SelectionList.BlockMenu.TextBoxFilter.Width = SelectionList.ActualWidth - 10;
            SelectionList.LiquidMenu.TextBoxFilter.Width = SelectionList.ActualWidth - 10;
            SelectionList.ObjectMenu.TextBoxFilter.Width = SelectionList.ActualWidth - 10;
        }
        catch { }

        //
    }
}
