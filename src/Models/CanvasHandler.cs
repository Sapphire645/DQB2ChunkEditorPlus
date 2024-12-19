using DQB2ChunkEditor.Controls;
using DQB2ChunkEditor.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;


namespace DQB2ChunkEditor.Models
{
    public class SelectionPencilClass
    {
        public short TileIdBeg { get; set; } = -1;
        public ushort x0 => (ushort)(TileIdBeg % 32);
        public ushort y0 => (ushort)(TileIdBeg / 32);
        public short LayerBeggining { get; set; } = -1;
        public ushort VirtualChunkBeggining { get; set; } = 0;
        public ushort Vx0 => (ushort)(VirtualChunkBeggining % 64);
        public ushort Vy0 => (ushort)(VirtualChunkBeggining / 64);
        public short TileIdEnd { get; set; } = -1;
        public ushort x1 => (ushort)(TileIdEnd % 32);
        public ushort y1 => (ushort)(TileIdEnd / 32);
        public short LayerEnd { get; set; } = -1;
        public ushort VirtualChunkEnd { get; set; } = 0;
        public ushort Vx1 => (ushort)(VirtualChunkEnd % 64);
        public ushort Vy1 => (ushort)(VirtualChunkEnd / 64);
        public bool HasArea { get { if (TileIdEnd >= 0 && TileIdBeg >= 0) return true; return false; } }
        public void Tile1(ushort tile, short layer, ushort VirtualChunk)
        {
            TileIdBeg = (short)tile;
            LayerBeggining = layer;
            VirtualChunkBeggining = VirtualChunk;
        }
        public void Tile2(ushort tile, short layer, ushort VirtualChunk)
        {
            TileIdEnd = (short)tile;
            LayerEnd = layer;
            VirtualChunkEnd = VirtualChunk;
        }
    }

    public class CanvasHandler
    {
        public SelectionPencilClass SelectedArea { get; set; } = new();
        public ObservableProperty<SelectionPencilClass> Test { get; set; } = new();
        public double coordLine { set; private get; }
        public Canvas SelectionCanvas;
        public Canvas BGCanvas;

        private List<Line> TileOne = new();
        private Ellipse PointOne = new();
        public List<Line> TileTwo = new();
        private Ellipse PointTwo = new();
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
        private void DrawTilePoint(ushort tile, List<Line> TileLines, bool Second, bool Top, bool Left)
        {
            TileLines.Clear();
            var x0 = tile % 32 * coordLine;
            var x1 = tile % 32 * coordLine + coordLine;
            var y0 = tile / 32 * coordLine;
            var y1 = tile / 32 * coordLine + coordLine;
            var Point = new Ellipse()
            {
                Width = 3,
                Height = 3,
                Fill = Brushes.Orange
            };
            if (Top)
            {
                DrawLine(x0, y0, x1, y0, TileLines);
                Canvas.SetTop(Point, y0);
            }
            else
            {
                DrawLine(x0, y1, x1, y1, TileLines);
                Canvas.SetTop(Point, y1);
            }
            if (Left)
            {
                DrawLine(x0, y0, x0, y1, TileLines);
                Canvas.SetLeft(Point, x0);
            }
            else { 
                DrawLine(x1, y0, x1, y1, TileLines);
                Canvas.SetLeft(Point, x1);
            }
            if (Second) PointTwo = Point;
            else PointOne = Point;
        }
        public void LineHandler(ushort tile,byte layer, uint VirtualChunk)
        {
            bool top = true;
            bool left = true;
            //Tile beggining
            if (SelectedArea.TileIdBeg == -1)
            {
                SelectedArea.Tile1(tile, layer, (ushort)VirtualChunk);
                if (SelectedArea.TileIdEnd == -1)
                    DrawTilePoint(tile, TileOne, false,true,true);
            }
            else if(SelectedArea.TileIdBeg == tile)
            {
                SelectedArea.TileIdBeg = -1;
            }
            //Tile end
            else if (SelectedArea.TileIdEnd == -1)
            {
                SelectedArea.Tile2(tile, layer, (ushort)VirtualChunk);
                if (SelectedArea.TileIdBeg == -1)
                    DrawTilePoint(tile, TileTwo, true, false, false);
            }else if (SelectedArea.TileIdEnd == tile)
            {
                SelectedArea.TileIdEnd = -1;
            }
            //Do the connecting.
            if (SelectedArea.TileIdBeg != -1 && SelectedArea.TileIdEnd != -1){
                if (SelectedArea.Vx0 == SelectedArea.Vx1)
                {
                    if (SelectedArea.x0 > SelectedArea.x1) left = false;
                }
                else
                    if (SelectedArea.Vx0 > SelectedArea.Vx1) left = false;
                if (SelectedArea.Vy0 == SelectedArea.Vy1)
                {
                    if (SelectedArea.y0 > SelectedArea.y1) top = false;
                }
                else
                        if (SelectedArea.Vy0 > SelectedArea.Vy1) top = false;
                DrawTilePoint((ushort)(SelectedArea.TileIdBeg), TileOne, false, top, left);
                DrawTilePoint((ushort)(SelectedArea.TileIdEnd), TileTwo, true, !top, !left);
            }
            UpdateCanvas(layer,VirtualChunk);
        }
        
        public void UpdateCanvas(byte layer, uint VirtualChunk)
        {
            double x0=-1,x1 = -1, y0 = -1, y1 = -1,aux; //rect
            SelectionCanvas.Children.Clear();
            BGCanvas.Children.Clear();
            if (SelectedArea.TileIdBeg != -1)
            {
                if (SelectedArea.VirtualChunkBeggining == VirtualChunk) //If same chunk beggining!
                {
                    x0 = SelectedArea.x0 * coordLine; //Point 1 rect
                    y0 = SelectedArea.y0 * coordLine;
                    if (SelectedArea.LayerBeggining == layer) // If same Y coord
                        foreach (var l in TileOne)
                            SelectionCanvas.Children.Add(l);
                    else
                        if (SelectedArea.LayerBeggining < layer && SelectedArea.LayerEnd >= layer ||
                        SelectedArea.LayerEnd < layer && SelectedArea.LayerBeggining >= layer) //if inside area
                        SelectionCanvas.Children.Add(PointOne);
                }
                else
                {
                    if (SelectedArea.Vx0 == VirtualChunk % 64) //same X coord
                        x0 = SelectedArea.x0 * coordLine; //x counts
                    else if (SelectedArea.Vx0 <= VirtualChunk % 64 && SelectedArea.Vx1 >= VirtualChunk % 64)
                        x0 = 0;
                    else if (SelectedArea.Vx1 <= VirtualChunk % 64 && SelectedArea.Vx0 >= VirtualChunk % 64)
                        x0 = 31 * coordLine;
                    else x0 = -1;
                    if (SelectedArea.Vy0 == VirtualChunk / 64) //same Y coord
                        y0 = SelectedArea.y0 * coordLine; //y counts
                    else if (SelectedArea.Vy0 <= VirtualChunk / 64 && SelectedArea.Vy1 >= VirtualChunk / 64)
                        y0 = 0;
                    else if (SelectedArea.Vy1 <= VirtualChunk / 64 && SelectedArea.Vy0 >= VirtualChunk / 64)
                        y0 = 31 * coordLine;
                    else y0 = -1;
                }
            }
            if (SelectedArea.TileIdEnd != -1)
                {
                    if (SelectedArea.VirtualChunkEnd == VirtualChunk) //If same chunk end!
                    {
                        x1 = SelectedArea.x1 * coordLine; //Point 2 rect
                        y1 = SelectedArea.y1 * coordLine;
                        if (SelectedArea.LayerEnd == layer) // If same Y coord
                            foreach (var l in TileTwo)
                                SelectionCanvas.Children.Add(l);
                        else
                            if (SelectedArea.LayerBeggining <= layer && SelectedArea.LayerEnd > layer ||
                            SelectedArea.LayerEnd <= layer && SelectedArea.LayerBeggining > layer) //if inside area
                            SelectionCanvas.Children.Add(PointTwo);
                    }
                    else
                    {
                        if (SelectedArea.Vx1 == VirtualChunk % 64) //same X coord
                            x1 = SelectedArea.x1 * coordLine; //x counts
                        else if (SelectedArea.Vx0 <= VirtualChunk % 64 && SelectedArea.Vx1 >= VirtualChunk % 64)
                            x1 = 31 * coordLine;
                        else if (SelectedArea.Vx1 <= VirtualChunk % 64 && SelectedArea.Vx0 >= VirtualChunk % 64)
                            x1 = 0;
                    else x1 = -1;
                        if (SelectedArea.Vy1 == VirtualChunk / 64) //same Y coord
                            y1 = SelectedArea.y1 * coordLine; //y counts
                        else if (SelectedArea.Vy0 <= VirtualChunk / 64 && SelectedArea.Vy1 >= VirtualChunk / 64)
                            y1 = 31 * coordLine;
                        else if (SelectedArea.Vy1 <= VirtualChunk / 64 && SelectedArea.Vy0 >= VirtualChunk / 64)
                            y1 = 0;
                        else y1 = -1;
                    }
                }
                if (SelectedArea.HasArea)
                {
                    if (y0 == -1 || x0 == -1 || y1 == -1 || x1 == -1) return;
                    if (x0 > x1) { aux = x0; x0 = x1; x1 = aux; }
                    if (y0 > y1) { aux = y0; y0 = y1; y1 = aux; }
                    Rectangle = new Rectangle()
                    {
                        Height = y1 - y0 + coordLine,
                        Width = x1 - x0 + coordLine,
                        Fill = Brushes.Yellow
                    };
                    Canvas.SetLeft(Rectangle, x0);
                    Canvas.SetTop(Rectangle, y0);
                    BGCanvas.Children.Add(Rectangle);
                }
        }
 
        public bool InsideArea(short Id)
        {
            if (SelectedArea == null) return true;
            if (!SelectedArea.HasArea) return true;
            if (SelectedArea.x0 <= Id%32 && SelectedArea.x1 >= Id % 32 && SelectedArea.y0 <= Id / 32 && SelectedArea.y1 >= Id / 32)
            {
                return true;
            }
            return false;
        }
    }
}
