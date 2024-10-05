using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DQB2ChunkEditor.Models;

public class Tile
{

    //----------- Identification ------------------
    [JsonPropertyName("Type")]
    public Type Type { get; set; } = Type.Block;

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
    public bool inList { get; set;} = false;

    //----------- Graphic ------------------

    [JsonPropertyName("ImageId")]
    public short ImageId { get; set; } = -1;

    [JsonPropertyName("Map")]
    public string ImagMap => $"/Images/Tiles/{ImageId:0000}.png";

    [JsonIgnore]
    public string Image => $"/Images/Blocks/{ImageId:0000}.png";

    public Tile() { }
    public void edit(Tile c)
    {
        ImageId = c.ImageId;
        Name = c.Name;
        Description = c.Description;
        NormalDrop = c.NormalDrop;
    }
}
