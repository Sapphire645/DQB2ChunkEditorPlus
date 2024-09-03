using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using DQB2ChunkEditor;

namespace DQB2ChunkEditor.Models;


//Names taken from Turtle_insect
public class ItemData
{
  public uint ItemID { get; }
  public uint PosX { get; }
  public uint PosY { get; }
  public uint PosZ { get; }

  public uint Chunk { get; }


//WRONG!!! DUNNO WHY THO :(
  public ItemData(uint chunkAddress) //BASE ADRESS: 0x24E7D1 + i * 24
                                                       //CHUNK ADRESS: 0x150E7D1 + i * 4
  {
    ItemID = ChunkEditor.GetItemData(chunkAddress + 2);
    ItemID = ItemID & 0xF0;

    PosY = ChunkEditor.GetItemData(chunkAddress + 2); //WRONG TOO 
    PosY = PosY & 0x0F;

    PosX = ChunkEditor.GetItemData(chunkAddress + 3);
    PosX = PosX & 0xF0;

    PosZ = ChunkEditor.GetItemData(chunkAddress + 3);
    PosZ = PosZ & 0x0f;

    Chunk = ChunkEditor.GetItemChunk(chunkAddress);
  }
}