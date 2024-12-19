using System;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace DQB2ChunkEditor.Models;

public class Tile
{
    public TileData TileData { get; set; }
    public ushort Id { get; set; }
    public bool BuilderPlaced => TileData.Id != Id;
    public ItemInstance ItemInstance { get; set; }
    public string ImagMap { get { return ItemInstance != null ? $"/Images/ObjectIcon/{ItemInstance.TileData.ImageId:0000}.png" : TileData != null ? $"/Images/Tiles/{TileData.ImageId:0000}.png" : $"/Images/Tiles/0000.png"; } }
    public bool seeThrough { get; set; } = false;
    public double ImageOpacity => seeThrough ? 0.5 : 1;
    public Tile() { }
    public Tile(TileData tileData)
    {
        TileData = tileData; 
    }
}

public class TileData
{
    public string Name { get; set; } = "Unknown";
    public short Colour { get; set; } = 0;
    public short Id { get; set; } = -1;
    public short ImageId { get; set; } = -1;
    public TileItemData ObjectData { get; set; } = null;

    public bool isObject => ObjectData != null;
    public bool isLiquid { get; set; } = false;
    public string Image { get { return ObjectData != null ? $"/Images/ObjectIcon/{ImageId:0000}.png" : $"/Images/Blocks/{ImageId:0000}.png"; } }
    public string ImagMap { get { return ObjectData != null ? $"/Images/ObjectIcon/{ImageId:0000}.png" : $"/Images/Tiles/{ImageId:0000}.png"; } }
    public TileData(short Id, short ImageId, bool isLiquid, short Colour, string Name, TileItemData ObjectData)
    {
        this.Id = Id;
        this.ImageId = ImageId;
        this.isLiquid = isLiquid;
        this.Colour = Colour;
        this.Name = Name;
        this.ObjectData = ObjectData;

    }
}
public class TileItemData
{
    public byte X { get; }
    public byte Y { get; }
    public byte Z { get; }

    public TileItemData(byte X, byte Y, byte Z)
    {
        this.X = X;
        this.Y = Y; 
        this.Z = Z;
    }

}
public class TileDataExtra
{
    public short Id { get; set; } = -1;

    public bool isObject { get; set; }
    //------------- Other -------------------
    public string Used
    {
        get
        {
            string value;
            if ((value = ExtractData(1)) != null && value != "0") return value;
            else return "Unknown";
        }
    }
    public string BrokenBy
    {
        get
        {
            string value;
            if ((value = ExtractData(2)) != null && value != "0") return value;
            else return "---";
        }
    }
    public string NormalDrop
    {
        get
        {
            string value;
            if ((value = ExtractData(3)) != null && value != "0") return value;
            else return "Nothing {-1}";
        }
    }
    public string UltimalletDrop
    {
        get
        {
            string value;
            if ((value = ExtractData(4)) != null && value != "0") return value;
            else return "Nothing {-1}";
        }
    }
    public string Description
    {
        get
        {
            string value;
            if ((value = ExtractData(5)) != null) return value;
            else return "---";
        }
    }
    //----------- Graphic ------------------
    public TileDataExtra(short ID)
    {
        Id = ID;
    }

    public TileDataExtra(short ID, bool Object)
    {
        Id = ID;
        isObject = Object;
    }
    public string ExtractData(int Type)
    {
        String line;
        if (isObject)
            //line = FileClass.ItemFile.Skip(Id + 1).FirstOrDefault();
            return "X";
        else
            line = FileClass.BlockFile.Skip(Id + 1).FirstOrDefault();
        String[] values = line.Split('\t');
        if (values.Length > Type)
            return values[Type];
        return null;
    }
}
