using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Windows.Shapes;

namespace DQB2ChunkEditor.Models;

public class SelectionPencilClass{

    public short TileIdBeg { get; set; } = -1;
    public ushort x0 => (ushort)(TileIdBeg%32);
    public ushort y0 => (ushort)(TileIdBeg/32);
    public short TileIdEnd { get; set; } = -1;
    public ushort x1 => (ushort)(TileIdEnd%32);
    public ushort y1 => (ushort)(TileIdEnd/32);

    public List<Line> BegList = new();
    public List<Line> EndList = new();
    public List<Line> RectList = new();
}