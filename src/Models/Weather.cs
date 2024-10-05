using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DQB2ChunkEditor.Models;

public class Weather        
{

    [JsonPropertyName("Id")]
    public ushort Id { get; set; } = 0;

    [JsonPropertyName("Name")]
    public string Name { get; set; } = "Null";

    [JsonPropertyName("Image")]
    public string Image => $"/Images/Weather/{Id:00}.png";

}

public class WeatherList
{
    [JsonPropertyName("Weather")]
    public List<Weather> Weathers { get; set; } = new List<Weather>();

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
