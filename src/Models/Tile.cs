using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DQB2ChunkEditor.Models;

public class Tile
{
    [JsonPropertyName("Type")]
    public bool isObject { get; set; } = false;

    [JsonPropertyName("Id")]
    public short Id { get; set; } = -1;

    [JsonPropertyName("Name")]
    public string Name { get; set; } = "Unknown";

    [JsonPropertyName("Description")]
    public string Description { get; set; } = "N/A";

    [JsonPropertyName("Normal Drop")]
    public string NormalDrop { get; set; } = "Nothing {-1}";

    [JsonPropertyName("Ultimallet Drop")]
    public string UltimalletDrop { get; set; } = "Nothing {-1}";

    //----------- Filtering ------------------

    [JsonPropertyName("Can be broken by")]
    public string Break { get; set; } = "N/A";

    [JsonPropertyName("Used")]
    public string Used { get; set; } = "N/A";

    [JsonPropertyName("Color")]
    public bool inList { get; set; } = false;

    //----------- Graphic ------------------

    [JsonPropertyName("ImageId")]
    public short ImageId { get; set; } = -1;

    [JsonPropertyName("Map")]
    public string ImagMap { get { return isObject ? $"/Images/ObjectIcon/{ImageId:0000}.png" : $"/Images/Tiles/{ImageId:0000}.png"; } }

    [JsonIgnore]
    public string Image { get { return isObject ? $"/Images/ObjectIcon/{ImageId:0000}.png" : $"/Images/Blocks/{ImageId:0000}.png"; } }

    [JsonPropertyName("BlockId")]
    public short mblockId { get; set; } = -1;

    [JsonIgnore]
    public short blockId { get { return isObject ? mblockId : (short)-1; } set { mblockId = value; } }
    [JsonIgnore]
    public short blockIdDisplay { get { return isObject ? blockId : Id; }}
    public Tile ShallowCopy()
    {
        return (Tile)this.MemberwiseClone();
    }
}
