using DQB2ChunkEditor.Controls;
using DQB2ChunkEditor.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;


namespace DQB2ChunkEditor.Models
{
    public class SelectionPencilClass
    {

        public short TileIdBeg { get; set; } = -1;
        public ushort x0 => (ushort)(TileIdBeg % 32);
        public ushort y0 => (ushort)(TileIdBeg / 32);
        public short TileIdEnd { get; set; } = -1;
        public ushort x1 => (ushort)(TileIdEnd % 32);
        public ushort y1 => (ushort)(TileIdEnd / 32);

        public short LayerBeggining { get; set; } = -1;
        public short LayerEnd { get; set; } = -1;
        public bool HasArea { get { if (TileIdEnd > 0 && TileIdBeg > 0) return true; return false; } }

        public List<Line> BegList = new();
        public List<Line> EndList = new();
    }

    public class CanvasHandler
    {
        public SelectionPencilClass SelectedArea { get; set; } = new();
        public ObservableProperty<SelectionPencilClass> Test { get; set; } = new();

        public Canvas SelectionCanvas;
        public Canvas BGCanvas;
        private Rectangle Rectangle;
        public void GridProcessing(UniformGrid Grid, short ChunkDisplayNumber)
        {
            Grid.Children.Clear();
            Grid.Columns = ChunkDisplayNumber * 4;
            Grid.Rows = ChunkDisplayNumber * 4;
            for (short i = 0; i < ChunkDisplayNumber * 16; i++)
                Grid.Children.Add(new Rectangle
                {
                    Stroke = Brushes.Black, // Set the border color
                    StrokeThickness = 1     // Set the thickness of the border
                });
        }
        /// <summary>
        /// Here is the selection handler with the magic pencil. First is the line.
        /// </summary>

        private void DrawLine(double x0, double y0, double x1, double y1, List<Line> List)
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
            List.Add(line);
        }

        /// <summary>
        /// Erases lines.
        /// </summary>
        private void EraseRect_OnClick(Object sender, RoutedEventArgs e)
        {
            EraseLine(SelectedArea.EndList);
            EraseLine(SelectedArea.BegList);
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
                DrawLine(x0 + ActualSize, y0, x0 + ActualSize, y0 + ActualSize, SelectedArea.BegList);
                DrawLine(x1, y1 + ActualSize, x1, y1, SelectedArea.EndList);
            }
            else
            {
                DrawLine(x0, y0, x0, y0 + ActualSize, SelectedArea.BegList);
                DrawLine(x1 + ActualSize, y1 + ActualSize, x1 + ActualSize, y1, SelectedArea.EndList);
            }
            if (y0 > y1)
            {
                changey = true;
                DrawLine(x0, y0 + ActualSize, x0 + ActualSize, y0 + ActualSize, SelectedArea.BegList);
                DrawLine(x1 + ActualSize, y1, x1, y1, SelectedArea.EndList);
            }
            else
            {
                DrawLine(x0, y0, x0 + ActualSize, y0, SelectedArea.BegList);
                DrawLine(x1 + ActualSize, y1 + ActualSize, x1, y1 + ActualSize, SelectedArea.EndList);
            }
            if (changex) { var temp = x0; x0 = x1; x1 = temp; }
            if (changey) { var temp = y0; y0 = y1; y1 = temp; }
            Rectangle = new Rectangle()
            {
                 Width = x1 + ActualSize - x0,
                 Height = y1 + ActualSize - y0,
                 Fill = Brushes.Yellow
            };
            Canvas.SetLeft(Rectangle, x0);
            Canvas.SetTop(Rectangle, y0);
            BGCanvas.Children.Add(Rectangle);
        }
        public void LineHandler(double x0, double y0, double coordLine, short Id, short Layer)
        {

            if (SelectedArea.TileIdBeg == Id)
            {
                SelectedArea.TileIdBeg = -1;
                SelectedArea.LayerBeggining = -1;
                EraseLine(SelectedArea.BegList);
            }
            else if (SelectedArea.TileIdEnd == Id)
            {

                SelectedArea.TileIdEnd = -1;
                SelectedArea.LayerEnd = -1;
                EraseLine(SelectedArea.EndList);
            }
            else if (SelectedArea.TileIdBeg == -1)
            {

                DrawLine(x0, y0, x0, y0 + coordLine, SelectedArea.BegList);
                DrawLine(x0, y0, x0 + coordLine, y0, SelectedArea.BegList);
                SelectedArea.TileIdBeg = Id;
                SelectedArea.LayerBeggining = Layer;
            }
            else
            {
                EraseLine(SelectedArea.EndList);
                BGCanvas.Children.Remove(Rectangle);
                DrawLine(x0 + coordLine, y0 + coordLine, x0, y0 + coordLine, SelectedArea.EndList);
                DrawLine(x0 + coordLine, y0 + coordLine, x0 + coordLine, y0, SelectedArea.EndList);
                SelectedArea.TileIdEnd = Id;
                SelectedArea.LayerEnd = Layer;
            }
            try
            {
                if (SelectedArea.TileIdEnd > -1 && SelectedArea.TileIdBeg > -1) DrawRectangle(coordLine);
                else { BGCanvas.Children.Remove(Rectangle); Rectangle = null; }
                }
            catch { }

            UpdateLayer((byte)Layer);
        }

        public void UpdateLayer(byte Layer)
        {
            if (SelectedArea != null)
            {
                try
                {
                    if (SelectedArea.LayerBeggining < Layer && SelectedArea.LayerEnd < Layer
                        || SelectedArea.LayerBeggining > Layer && SelectedArea.LayerEnd > Layer)
                    {
                        BGCanvas.Children.Remove(Rectangle);
                    }
                    else
                    {
                        try
                        {
                            BGCanvas.Children.Add(Rectangle);
                        }
                        catch { }
                    }
                    if (SelectedArea.LayerBeggining != Layer)
                    {
                        foreach (var line in SelectedArea.BegList)
                            SelectionCanvas.Children.Remove(line);
                    }
                    else
                    {
                        foreach (var line in SelectedArea.BegList)
                            SelectionCanvas.Children.Add(line);
                    }
                    if (SelectedArea.LayerEnd != Layer)
                    {
                        foreach (var line in SelectedArea.EndList)
                            SelectionCanvas.Children.Remove(line);
                    }
                    else
                    {
                        foreach (var line in SelectedArea.EndList)
                            SelectionCanvas.Children.Add(line);
                    }
                }
                catch { }
            }
        }
    }
}
