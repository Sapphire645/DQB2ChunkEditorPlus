using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DQB2ChunkEditor.Controls
{
    /// <summary>
    /// Interaction logic for ChunkGrid.xaml
    /// </summary>
    public partial class ChunkGrid : UserControl
    {
        public ObservableProperty<BitmapImage> LastBitmap { get; private set; } = new ObservableProperty<BitmapImage>();

        public event Action<short> ChunkClick;

        private List<uint> ChunkGridList = new List<uint>();
        private List<uint> ChunkGridListOld = new List<uint>();
        private ObservableProperty<short> ChunkCount = new ObservableProperty<short>();

        private bool editing = false;
        private uint[] ints;
        private short ImageToggle = 0;
        public ChunkGrid()
        {
            InitializeComponent();
        }
        public void CreateMainGrid()
        {
            try
            {
                ChunkCount.Value = ChunkEditor.VirtualChunkCount;
                ChunkGridList.Clear();
                var Uri = new Uri("/Images/Maps/STGDAT" + ChunkEditor.Island.ToString("D2") + ".png", UriKind.Relative);
                if (Uri == null) {
                    Uri = new Uri("pack://application:,,,/Images/Maps/STGDAT" + ChunkEditor.Island.ToString("D2") + ".png");
                }
                LastBitmap.Value = new BitmapImage(Uri);
                ImageBackground.Source = LastBitmap.Value;
                MapGrid.Children.Clear();
                ints = ChunkEditor.GetChunkCrop();
                MapGrid.Rows = (int)(ints[1] - ints[0] + 3);
                MapGrid.Columns = (int)(ints[3] - ints[2] + 3);
                for (short i = 0; i < 64 * 64; i++) // layers are 32x32
                {
                    var val = ChunkEditor.GetChunkFromGrid(i);
                    if ((i / 64) >= (ints[0] - 1) && (i / 64) <= (ints[1] + 1) && (i % 64) >= (ints[2] - 1) && (i % 64) <= (ints[3] + 1))
                    {
                        var button = new Button()
                        {
                            Tag = i,
                            Height = 64,
                            Width = 64,
                            Background = Brushes.Transparent,
                            FontSize = 35,
                            FontWeight =  FontWeights.Bold
                        };
                        button.Click += (_, _) => { Chunk_Click(button); };
                        if (val == 0xFFFF)
                        {
                            button.Background = Brushes.Gray;
                        }
                        else
                        {
                            button.Content = val;
                        }
                        MapGrid.Children.Add(button);
                    }
                    ChunkGridList.Add(val);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        public void CreateMainGridEdit()
        {
            try
            {
                ChunkCount.Value = ChunkEditor.VirtualChunkCount;
                ChunkGridList.Clear();
                ChunkGridListOld.Clear();
                ImageBackground.Source = null;
                MapGrid.Children.Clear();
                MapGrid.Rows = 64;
                MapGrid.Columns = 64;
                for (short i = 0; i < 64 * 64; i++) 
                {
                    var val = ChunkEditor.GetChunkFromGrid(i);
                    var button = new Button()
                    {
                        Tag = i,
                        Height = 64,
                        Width = 64,
                        Background = Brushes.Transparent,
                        FontSize = 35,
                        FontWeight = FontWeights.Bold
                    };
                    button.Click += (_, _) => { Chunk_Click(button); };
                    ChunkGridList.Add(val);
                    ChunkGridListOld.Add(val);
                    if (val == 0xFFFF)
                    {
                        button.Background = Brushes.Gray;
                    }
                    else
                    {
                        button.Content = val;
                    }
                    MapGrid.Children.Add(button);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        private void Chunk_Click(Button sender)
        {
            short offset = Convert.ToInt16(sender.Tag);
            if (editing)
            {
                if(ChunkGridList.ElementAt(offset) == 0xFFFF)
                {
                    if(ChunkCount.Value < 700)
                    {
                        ChunkGridList[offset] = (uint)ChunkCount.Value;
                        sender.Content = ChunkCount.Value;
                        sender.Background = Brushes.Transparent;
                        ChunkCount.Value += 1;
                    }
                }
                else
                {
                    ChunkCount.Value -= 1;
                    var CuttingLine = ChunkGridList[offset];
                    ChunkGridList[offset] = 0xFFFF;
                    sender.Background = Brushes.Gray;
                    sender.Content = "";
                    var i = 0;
                    foreach (Button Button in MapGrid.Children)
                    {
                        if (ChunkGridList[i] != 0xFFFF && ChunkGridList[i] > CuttingLine)
                        {
                            Button.Content = ChunkGridList[i] - 1;
                            ChunkGridList[i] = ChunkGridList[i] - 1;
                        }
                        i++;
                    }
                }
            }
            else
            {
                if (ChunkGridList[offset] != 0xFFFF)
                {
                    ChunkClick?.Invoke((short)ChunkGridList[offset]);
                }
            }
        }
        private void Editing(object sender, EventArgs e)
        {
            var Button = sender as Button;
            if (editing)
            {
                Button.Content = "Edit";
                CancelButton.Visibility = Visibility.Collapsed;
                editing = false;
                ChunkEditor.ChunkGridEdit(ChunkGridList, ChunkGridListOld, ChunkCount.Value);
                CreateMainGrid();
            }
            else
            {
                Button.Content = "Confirm Edits";
                CancelButton.Visibility= Visibility.Visible;
                editing = true;
                CreateMainGridEdit();
            }
        }
        private void Cancel(object sender, EventArgs e)
        {
            EditButton.Content = "Edit";
            CancelButton.Visibility = Visibility.Collapsed;
            editing = false;
            CreateMainGrid();
        }
        public void UpdateSelection(bool selection,ushort VChunk1, ushort VChunk2, ushort x1, ushort y1, ushort x2, ushort y2)
        {
            if (selection)
            {
                SelectionRectangle.Visibility = Visibility.Visible;
                var coord = Math.Min(VBox.ActualHeight / MapGrid.Rows, VBox.ActualWidth / MapGrid.Columns);
                System.Windows.Point MapGridPos = MapGrid.TransformToAncestor(this).Transform(new System.Windows.Point(0, 0));
                var x = Math.Min(((VChunk1 % 64) - ints[2]) * coord + (x1 * coord / 32), ((VChunk2 % 64) - ints[2]) * coord + (x2 * coord / 32));
                var y = Math.Min(((VChunk1 / 64) - ints[0]) * coord + (y1 * coord / 32), ((VChunk2 / 64) - ints[0]) * coord + (y2 * coord / 32));

                var Mx = Math.Max(((VChunk1 % 64) - ints[2]) * coord + (x1 * coord / 32), ((VChunk2 % 64) - ints[2]) * coord + (x2 * coord / 32));
                var My = Math.Max(((VChunk1 / 64) - ints[0]) * coord + (y1 * coord / 32), ((VChunk2 / 64) - ints[0]) * coord + (y2 * coord / 32));

                SelectionRectangle.Margin = new Thickness(MapGridPos.X - GridOneCol.ActualWidth + (x + coord), MapGridPos.Y + (y + coord), 0, 0);
                SelectionRectangle.Width = Mx - x;
                SelectionRectangle.Height = My - y;
            }
            else{
                SelectionRectangle.Visibility = Visibility.Collapsed;
            }
        }
        private void Image(object sender, EventArgs e)
        {
            Uri Uri;
            ImageToggle += 1;
            switch (ImageToggle)
            {
                case 0:
                    Uri = new Uri("/Images/Maps/STGDAT" + ChunkEditor.Island.ToString("D2") + ".png", UriKind.Relative);
                    if (Uri == null)
                    {
                        try
                        {
                            Uri = new Uri("pack://application:,,,/Images/Maps/STGDAT" + ChunkEditor.Island.ToString("D2") + ".png");
                        }
                        catch
                        {

                        }
                        ImageToggle += 1;
                    }
                    LastBitmap.Value = new BitmapImage(Uri);
                    ImageBackground.Source = LastBitmap.Value;
                    break;
                case 1:
                    try
                    {
                        Uri = new Uri("pack://application:,,,/Images/Maps/STGDAT" + ChunkEditor.Island.ToString("D2") + ".png");
                    }
                    catch
                    {
                        Uri = null;
                    }
                    LastBitmap.Value = new BitmapImage(Uri);
                    ImageBackground.Source = LastBitmap.Value;
                    break;
                case 2:
                    ImageBackground.Source = null;
                    ImageToggle = -1;
                    break;
            }
        }
    }
}
