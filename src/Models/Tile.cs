using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DQB2ChunkEditor.Models;

public class Tile
{
    [JsonPropertyName("Type")]
    public Type Type { get; set; } = Type.Block;

    [JsonPropertyName("ListId")]
    public short ListId => (short)(Id % 2048);

    [JsonPropertyName("Is Overflowed")]
    public bool Overflow => Id > 2047;

    [JsonPropertyName("Id")]
    public short Id { get; set; } = -1;

    [JsonPropertyName("ImageId")]
    public short ImageId { get; set; } = -1;

    [JsonPropertyName("Name")]
    public string Name { get; set; } = "Unknown";

    [JsonPropertyName("Description")]
    public string Description { get; set; } = "N/A";

    [JsonPropertyName("Can be broken by")]
    public string Break { get; set; } = "N/A";

    [JsonPropertyName("Used")]
    public string Used { get; set; } = "N/A";

    [JsonPropertyName("Color")]
    public bool inList { get; set;} = false;

    [JsonPropertyName("Map")]
    public string ImagMap => $"/Images/Tiles/{ImageId:0000}.png";

    [JsonIgnore]
    public string Image => $"/Images/Blocks/{ImageId:0000}.png";
}
