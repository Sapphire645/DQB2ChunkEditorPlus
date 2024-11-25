using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Controls;
using System.Windows.Media;

namespace DQBChunkEditor.Models;

public class Tile
{
    public TileData TileData { get; set; }
    public ushort Id { get; set; }
    public ItemInstance ItemInstance { get; set; }
    public bool seeThrough { get; set; } = false;

    public bool BuilderPlaced => TileData.Id != Id;
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
    public bool isObject { get; set; } = false;
    public bool isLiquid { get; set; } = false;
    public short tab { get; set; } = 0;
    public string ImagMap { get { return isObject ? $"/Images/ObjectIcon/{ImageId:0000}.png" : $"/Images/Tiles/{ImageId:0000}.png"; } }
    public string Image { get { return isObject ? $"/Images/ObjectIcon/{ImageId:0000}.png" : $"/Images/Blocks/{ImageId:0000}.png"; } }
    public TileDataExtra Extras { get; set; }

    public TileData(short ID)
    {
        Id = ID;
        Extras = new TileDataExtra(ID);
        short.TryParse(Extras.ExtractData(1), out var shortVar);
        ImageId = shortVar;
        Name = Extras.ExtractData(3);
    }
    public TileData(short ID, bool Object)
    {
        Id = ID;
        isObject = Object;
        Extras = new TileDataExtra(ID,Object);
        short.TryParse(Extras.ExtractData(1), out var shortVar);
        ImageId = shortVar;
        Name = Extras.ExtractData(3);
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
            if ((value = ExtractData(2)) != null && value != "0") return value;
            else return "Unknown";
        }
    }
    public string BrokenBy
    {
        get
        {
            string value;
            if ((value = ExtractData(4)) != null && value != "0") return value;
            else return "---";
        }
    }
    public string NormalDrop
    {
        get
        {
            string value;
            if ((value = ExtractData(5)) != null && value != "0") return value;
            else return "Nothing {-1}";
        }
    }
    public string UltimalletDrop
    {
        get
        {
            string value;
            if ((value = ExtractData(6)) != null && value != "0") return value;
            else return "Nothing {-1}";
        }
    }
    public string Description
    {
        get
        {
            string value;
            if ((value = ExtractData(7)) != null) return value;
            else return "---";
        }
    }
    //----------- Graphic ------------------
    public short itemBlockId
    {
        get
        {
            string value;
            if ((value = ExtractData(8)) != null)
                if (short.TryParse(value, out var shortVar))
                    return shortVar;
            return -1;
        }
    }
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
            line = FileClass.ItemFile.Skip(Id + 1).FirstOrDefault();
        else
            line = FileClass.BlockFile.Skip(Id + 1).FirstOrDefault();
        String[] values = line.Split('\t');
        if (values.Length > Type)
            return values[Type];
        return null;
    }
}
