﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DQB2ChunkEditor.Models;

public class TileList
{
    [JsonPropertyName("Tiles")]
    public List<Tile> Tiles { get; set; } = new List<Tile>();

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
