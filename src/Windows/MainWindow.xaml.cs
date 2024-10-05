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
using System.Globalization;
using System.Windows.Data;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Threading.Tasks;

namespace DQB2ChunkEditor.Windows;

public partial class MainWindow : Window
{
    public static List<Tile> TileList { get; } = new();
    public static ObservableCollection<ComboBoxTile> TileComboBoxList { get; set; } = new();
    public List<Weather> WeatherList { get; set; } = new();
    public ObservableCollection<ComboBoxWeather> WeatherComboBox { get; set; } = new();
    public static ObservableProperty<Tile> SelectedTile { get; set; } = new();

    public Tile selectedTileDrop { get; set; } = new();
    public ObservableProperty<LayerTile> SelectedLayerTile { get; set; } = new();
    public List<ItemData> ItemDataList { get; set; } = new();
    public SelectionPencilClass SelectedArea { get; set; } = new();
    public ObservableProperty<short> ChunkValue { get; set; } = new() { Value = 0 };
    public ObservableProperty<byte> LayerValue { get; set; } = new() { Value = 0 };

    public static short Overflow { get; set; } = 0;
    public static short RawID { get; set; } = 0;

    public static short[] LimitSelector { get; set; } = new short[4];

    public static BlockGrid ButtonGridReplace = new BlockGrid();

    public ObservableCollection<SignText> SignTextList { get; set; } = new ObservableCollection<SignText>();

    public MainWindow()
    {
        InitializeComponent();
        CreateDefaultTiles();
        CreateComboBoxTiles();
        CreateComboBoxTilesWeather();
        DataContext = this;
        this.SizeChanged += OnWindowSizeChanged;
        ButtonGrid.ButtonClicked += ButtonClick;
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
            var TextBox = new Preview(Line);
            Grid.SetColumn(TextBox, 1);
            Grid.SetRow(TextBox, 1);
            Pain.Children.Add(TextBox);
        }
    }
    /// <summary>
    /// Selection to combobox (temporal)
    /// </summary>
    public void ButtonClick(object sender, EventArgs e)
    {
        var clickedButton = (sender as BlockGrid).clickedButton;
        TileComboBox.SelectedIndex = (short)TileComboBoxList.FirstOrDefault(t => t.Tile.Id == clickedButton.Tile.Id)!.Id;
    }
    /// <summary>
    /// Resize of everything
    /// </summary>
    protected void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
    {
        double newWindowHeight = e.NewSize.Height;
        double newWindowWidth = e.NewSize.Width;
        One.Width = new GridLength((newWindowWidth-newWindowHeight), GridUnitType.Pixel);
        Two.Width = new GridLength(newWindowHeight-50, GridUnitType.Pixel);
        Select.Height = new GridLength((newWindowHeight-380), GridUnitType.Pixel);
        ButtonGrid.ScrollView.Height = (newWindowHeight - 400);
        ButtonGrid.Grid.Columns = Convert.ToInt16(newWindowWidth - newWindowHeight) / 100;
    }


    private void ReadSign()
    {
        SignTextList = ChunkEditor.LoadSign();
        List.ItemsSource = SignTextList;

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
            ButtonGrid.CreateButtons();
            ButtonGridReplace.CreateButtons();
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
    /// Refreshes tiles
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
                ((LayerTile)LayerTiles.Children[i]).Tile.Value = TileValue;
            }

            ItemDataList = new();
            for (short i = 0; i < ChunkEditor.ItemCount; i++)
            {
                var ChunkItem = ChunkEditor.GetItemChunk((uint)(0x150E7D1 + i * 4));
                if ((short)ChunkItem == chunk)
                {
                    var ItemData = new ItemData((uint)(0x150E7D1 + i * 4));
                    if (ItemData.PosY == (uint)layer)
                    {
                        ItemDataList.Add(ItemData);
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
    /// Here is the selection handler with the magic pencil. First is the line.
    /// </summary>

    private void DrawLine(double x0, double y0, double x1, double y1, List<Line> List, bool yellow)
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
        if (yellow) line.Stroke = Brushes.Yellow;

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
            DrawLine(x1 + ActualSize, y1 + ActualSize, x1, y1 + ActualSize, SelectedArea.EndList, false);
        }

        if (changey == changex)
        {
            if (changex == true)
            {
                var xa = x0; x0 = x1; x1 = xa;
                xa = y0; y0 = y1; y1 = xa;
            }
            DrawLine(x0 + ActualSize, y0, x1 + ActualSize, y0, SelectedArea.RectList, true);
            DrawLine(x0, y1 + ActualSize, x1, y1 + ActualSize, SelectedArea.RectList, true);
            DrawLine(x0, y0 + ActualSize, x0, y1 + ActualSize, SelectedArea.RectList, true);
            DrawLine(x1 + ActualSize, y0, x1 + ActualSize, y1, SelectedArea.RectList, true);

        }
        else
        {
            if (changex == true)
            {
                var xa = x0; x0 = x1; x1 = xa;
                xa = y0; y0 = y1; y1 = xa;
            }
            DrawLine(x0 + ActualSize, y0 + ActualSize, x1 + ActualSize, y0 + ActualSize, SelectedArea.RectList, true);
            DrawLine(x0, y1, x1, y1, SelectedArea.RectList, true);
            DrawLine(x0, y0, x0, y1, SelectedArea.RectList, true);
            DrawLine(x1 + ActualSize, y0 + ActualSize, x1 + ActualSize, y1 + ActualSize, SelectedArea.RectList, true);
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
                var IDcheck = (short)ChunkEditor.GetBlockValue(ChunkValue.Value, LayerValue.Value, SelectedLayerTile.Value.Id);
                if (IDcheck >= 2048)
                {
                    overflowCheckboxName.IsChecked = true;
                    Overflow = 2048;
                }
                else
                {
                    overflowCheckboxName.IsChecked = false;
                    Overflow = 0;
                }
                TileComboBox.SelectedIndex = (short)TileComboBoxList.FirstOrDefault(t => t.Tile.Id == layerTile.Tile.Value.Id)!.Id;
                RawID = (short)(SelectedTile.Value.Id + Overflow);
                RawIDT.Text = "RawID: " + Convert.ToString(RawID);
                OverflowT.Text = "Overflow: " + Convert.ToString(Overflow);

            }
            // If it's the paste button just update the tile with the current selected value
            else if (PasteButton.IsChecked == true)
            {
                ((LayerTile)LayerTiles.Children[SelectedLayerTile.Value!.Id]).Tile.Value = SelectedTile.Value;
                ChunkEditor.SetBlockValue(ChunkValue.Value, LayerValue.Value, SelectedLayerTile.Value.Id, (short)(SelectedLayerTile.Value.Id + Overflow));
            }
            // If it's the select button do select logic
            else if (PencilButton.IsChecked == true)
            {
                var coordLine = ((LayerTile)LayerTiles.Children[SelectedLayerTile.Value!.Id]).ActualHeight;
                var x0 = (SelectedLayerTile.Value.Id % 32) * coordLine;
                var y0 = (SelectedLayerTile.Value.Id / 32) * coordLine;

                if (SelectedArea.TileIdBeg == SelectedLayerTile.Value.Id)
                {

                    SelectedArea.TileIdBeg = -1;
                    EraseLine(SelectedArea.BegList);
                }
                else if (SelectedArea.TileIdEnd == SelectedLayerTile.Value.Id)
                {

                    SelectedArea.TileIdEnd = -1;
                    EraseLine(SelectedArea.EndList);
                }
                else if (SelectedArea.TileIdBeg == -1)
                {

                    DrawLine(x0, y0, x0, y0 + coordLine, SelectedArea.BegList, false);
                    DrawLine(x0, y0, x0 + coordLine, y0, SelectedArea.BegList, false);
                    SelectedArea.TileIdBeg = SelectedLayerTile.Value.Id;
                }
                else
                {
                    EraseLine(SelectedArea.EndList);
                    EraseLine(SelectedArea.RectList);
                    DrawLine(x0 + coordLine, y0 + coordLine, x0, y0 + coordLine, SelectedArea.EndList, false);
                    DrawLine(x0 + coordLine, y0 + coordLine, x0 + coordLine, y0, SelectedArea.EndList, false);
                    SelectedArea.TileIdEnd = SelectedLayerTile.Value.Id;
                }
                if (SelectedArea.TileIdEnd > -1 && SelectedArea.TileIdBeg > -1) DrawRectangle(coordLine);
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
    private void UpdateIdSelected_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var change = ((LayerTile)LayerTiles.Children[SelectedLayerTile.Value.Id]).Tile.Value;

            if (overflowCheckboxName.IsChecked == true)
            {

                Overflow = 2048;
            }
            else Overflow = 0;
            RawID = (short)(change.Id + Overflow);
            RawIDT.Text = "RawID: " + Convert.ToString(RawID);
            OverflowT.Text = "Overflow: " + Convert.ToString(Overflow);

            ChunkEditor.SetBlockValue(ChunkValue.Value, LayerValue.Value, SelectedLayerTile.Value.Id, (short)(change.Id + Overflow));
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


            if (PencilButton.IsChecked == true)
            {
                ChunkEditor.SetAreaValue(ChunkValue.Value, LayerValue.Value, SelectedArea.TileIdBeg, SelectedArea.TileIdEnd, (short)(selectedTileDrop.Id + Overflow));
                RefreshTiles(ChunkValue.Value, LayerValue.Value);
            }
            RawID = (short)(SelectedTile.Value.Id + Overflow);
            RawIDT.Text = "RawID: " + Convert.ToString(RawID);
            OverflowT.Text = "Overflow: " + Convert.ToString(Overflow);
            ChunkEditor.SetBlockValue(ChunkValue.Value, LayerValue.Value, SelectedLayerTile.Value.Id, (short)(selectedTileDrop.Id + Overflow));
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
            if (ChunkValue.Value >= ChunkEditor.ChunkCount)
            {
                ChunkValue.Value = ChunkEditor.ChunkCount;
            }

            RefreshTiles(ChunkValue.Value, LayerValue.Value);
            GratitudeInput.Text = Convert.ToString(ChunkEditor.GratitudePoints);
            TimeInput.Text = Convert.ToString(ChunkEditor.Clock);
            WeatherComboBoxX.SelectedIndex = ChunkEditor.Weather;
            LimitSelector[0] = 0;
            LimitSelector[1] = (short)ChunkEditor.ChunkCount;
            LimitSelector[2] = 0;
            LimitSelector[3] = (short)ChunkEditor.LayerHeight;
            ReadSign();
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
            GratitudeInput.Text = Convert.ToString(ChunkEditor.GratitudePoints);
            TimeInput.Text = Convert.ToString(ChunkEditor.Clock);
            WeatherComboBoxX.SelectedIndex = ChunkEditor.Weather;
            ReadSign();
            LayerTiles.Visibility = Visibility.Visible;
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
    private void MassSet_OnClick(Object sender, RoutedEventArgs e)
    {
        try
        {
            var ConfirmR = new ConfirmChoice
        {
            Text = "Set block on mass selected area?"
        };

        if (ConfirmR.ShowDialog() == false)
        {
            return;
        }
        if (ConfirmR.Confirmed == true)
        {
            ChunkEditor.MassSetBlockValue(LimitSelector[0], LimitSelector[1], (byte)LimitSelector[2], LimitSelector[3], SelectedTile.Value.Id);
            RefreshTiles(ChunkValue.Value, LayerValue.Value);
        }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private void ReplaceBlocks_OnClick(Object sender, RoutedEventArgs e)
    {
        try
        {
            var ReplaceBlockWindow = new ReplaceBlock(ButtonGridReplace)
            {
                FirstId = -1,
                SecondId = -1
            };

            if (ReplaceBlockWindow.ShowDialog() == false)
            {
                ReplaceBlockWindow.GridAdd.Children.Clear();
                return;
            }
            ReplaceBlockWindow.GridAdd.Children.Clear();
            string TextM = "N/A";
            if (PencilButton.IsChecked == true)
            {
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
            if (ConfirmR.Confirmed == true)
            {
                if (PencilButton.IsChecked == true) ChunkEditor.ReplaceAreaValue(ChunkValue.Value, LayerValue.Value, SelectedArea.TileIdBeg, SelectedArea.TileIdEnd, ReplaceBlockWindow.FirstId, ReplaceBlockWindow.SecondId);
                else ChunkEditor.ReplaceBlockValue(LimitSelector[0], LimitSelector[1], (byte)LimitSelector[2], LimitSelector[3], ReplaceBlockWindow.FirstId, ReplaceBlockWindow.SecondId);
                RefreshTiles(ChunkValue.Value, LayerValue.Value);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    //For some reason Turtle's flattener doesn't work for me... And I need it for testing. 
    private void Flattener_OnClick(Object sender, RoutedEventArgs e)
    {
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
    private bool IsLiquid(Tile block)
    {
            if ((block.Name.ToLower().Contains("water") || block.Name.Contains("Plasma") || block.Name.Contains("Bottomless") || block.Name.Contains("Poison ") || block.Name.Contains("Lava "))
                && block.Id != 593 && block.Id != 17 && block.Id != 86) { return true; }
            else { return false; }
    }
    private async void Drainer_OnClick(Object sender, RoutedEventArgs e)
    {
        try
        {
            var ConfirmF = new ConfirmChoice
            {
                Text = "This will delete all liquid, and leave items inside liquid as ghost items. It will also take a loooong time.  Are you sure you want to continue?"
            };

            if (ConfirmF.ShowDialog() == false)
            {
                return;
            }
            Loading.Visibility = Visibility.Visible;
            MainGrid.IsEnabled = false;
            var a= PencilButton.IsChecked;
            await Task.Run(() => ReplaceLiquidOperation(a));
            RefreshTiles(ChunkValue.Value, LayerValue.Value);
            MainGrid.IsEnabled = true; 
            Loading.Visibility = Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
    private void ReplaceLiquidOperation(bool? penButton)
    {
        List<short> Lis = new List<short>();
        foreach (var tile in TileList)
        {
            if (IsLiquid(tile))
            {
                Lis.Add(tile.Id);
            }
        }
        if (penButton == true)
        {
                ChunkEditor.KillLiquid(LimitSelector[0], LimitSelector[1], LayerValue.Value, LayerValue.Value, Lis);
        }
        else
        {
                ChunkEditor.KillLiquid(LimitSelector[0], LimitSelector[1], (byte)LimitSelector[2], LimitSelector[3], Lis);
        }
    }

        private void Print_OnClick(Object sender, RoutedEventArgs e)
    {
        try
        {
            var ConfirmF = new ConfirmChoice
            {
                Text = "This prints out the block counts into a TXT file."
            };

            if (ConfirmF.ShowDialog() == false)
            {
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Text file (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if (saveFileDialog.ShowDialog() == false)
            {
                return;
            }

            var List = ChunkEditor.CountBlockData();
            using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
            {
                for (int i = 0; i < 2048; i++)
                {
                    writer.WriteLine(List[i] + "\t" + TileList[i].Name);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private void Size_OnClick(Object sender, RoutedEventArgs e)
    {
        try
        {
            var ValueEditor = new ValueEditor
            {
                Value = (uint)ChunkEditor.ChunkCount,
                Text = "Chunk Count [TEMPORAL FIX, ONLY CHANGE THIS IF YOU KNOW THE VALUE IS WRONG. 'REPLACE ALL' AND 'FLATTENER' WILL CORRUPT THE SAVE IF THE CHUNK COUNT IS BIGGER THAN THE REAL VALUE]",
                ImagePath = ""
            };

            if (ValueEditor.ShowDialog() == false || !short.TryParse(ValueEditor.ResponseText, out var value))
            {

                return;
            }
            if (value >= 1 && value <= 800)
            {
                ChunkEditor.ChunkCount = value;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
    private void GratitudeInput_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            if (!UInt32.TryParse(GratitudeInput.Text, out var value))
            {
                return;
            }
            else
            {
                ChunkEditor.GratitudePoints = value;
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

    private void MassSelect_OnClick(object sender, RoutedEventArgs e)
    {
        var Select = new MassSelect
        {
        };

        if (Select.ShowDialog() == false)
        {
            return;
        }
        short.TryParse(Select.BegC, out LimitSelector[0]);
        short.TryParse(Select.EndC, out LimitSelector[1]);
        short.TryParse(Select.BegY, out LimitSelector[2]);
        short.TryParse(Select.EndY, out LimitSelector[3]);
    }
}


