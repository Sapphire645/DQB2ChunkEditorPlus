using System;
using System.Buffers.Binary;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using DQB2ChunkEditor.Models;
using DQB2ChunkEditor.Windows;
using Ionic.Zlib;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.Windows.Documents;


namespace DQB2ChunkEditor;

public static class ChunkEditor
{
    private const uint LayerSize = 0x800; // layers are 32x32 * 2
    private const uint HeaderLength = 0x110; // everything after the header is compressed data
    private const uint ChunkSize = 0x30000; // chunk size is layer size (1024) * layer height (96) * block size (2)

    private const uint ChunkGridSize = 0x2000;
    private const uint ItemSize = 0x12C0000;
    private const uint ItemOffsetSize = 0x320000;

    private const uint ChunkGridStart = 0x24C7C1; // starting index for the chunk offsets
    private const uint ItemStart = 0x24E7D1; // starting index for the item data
    private const uint ItemOffsetStart = 0x150E7D1; // starting index for the chunk offsets
    private const uint BlockStart = 0x183FEF0; // starting index for the block data
    private const string ExpectedFileHeader = "aerC"; // expected file header for the map

    private static byte[] _headerBytes = new byte[HeaderLength];
    private static byte[] _uncompressedBytes = null!;

    public static string Filename { get; set; } = null!;
    public const uint LayerHeight = 0x60; // layers are up to 96 blocks high

    public static string Pname { get; set; } = null!;
    public static UInt32 GratitudePoints { get; set; } = 99999; //gratitude points
    public static float Clock { get; set; } = 0; //clock
    public static ushort Weather { get; set; } = 1; //weather
    public static short VirtualChunkCount { get; set; } = 0; // chunk count changes per map
    public static short ChunkCount { get; set; } = 0; // chunk count changes per map
    public static byte Island { get; set; } = 0xFF; //Island Number
    public static byte IslandType { get; set; } = 0xFF; //Type of Island
    public static uint VirtualItemCount { get; private set; } = 0;
    public static uint TemporalItemCount { get; private set; } = 0;
    public static uint TemporalSeaLevel { get; private set; } = 0;

    private static List<List<(uint, ushort)>> ChunkItems; //chunk, layer.

    public static List<TileData> ObjectList { private get; set; }
    public static void LoadFile(string filename)
    {
        var fileBytes = System.IO.File.ReadAllBytes(filename);
        var headerBytes = new byte[4];
        Array.Copy(fileBytes, 0, headerBytes, 0, 4);

        var actualHeader = Encoding.UTF8.GetString(headerBytes);

        if (ExpectedFileHeader != actualHeader)
        {
            return;
        }
        var compressedBytes = new byte[fileBytes.Length - HeaderLength];

        Array.Copy(fileBytes, HeaderLength, compressedBytes, 0, compressedBytes.Length);
        Array.Copy(fileBytes, _headerBytes, HeaderLength);

        using (var input = new MemoryStream(compressedBytes))
        {
            using (var zlib = new System.IO.Compression.ZLibStream(input, System.IO.Compression.CompressionMode.Decompress))
            {
                using (var output = new MemoryStream())
                {
                    zlib.CopyTo(output);
                    _uncompressedBytes = output.ToArray();
                }
            }
        }
        LoadFileCommon(filename);
    }
    private static void LoadFileCommon(string filename)
    {
        var chunkBytes = new byte[2];
        chunkBytes[0] = _uncompressedBytes[0x1451af];
        chunkBytes[1] = _uncompressedBytes[0x1451B0];
        ChunkCount = (short)(BitConverter.ToUInt16(chunkBytes));
        chunkBytes[0] = _uncompressedBytes[0x24e7c5];
        chunkBytes[1] = _uncompressedBytes[0x24e7c6];
        VirtualChunkCount = (short)(BitConverter.ToUInt16(chunkBytes));
        chunkBytes[0] = _uncompressedBytes[0x24e7cD];
        chunkBytes[1] = _uncompressedBytes[0x24e7cE];
        VirtualItemCount = BitConverter.ToUInt16(chunkBytes);
        Filename = filename;
        Island = _uncompressedBytes[0xC0FE6 - HeaderLength];
        IslandType = _uncompressedBytes[0xC0FE6 - HeaderLength];
        ReadAllItems();
        readExtra();
        var a = System.IO.File.ReadLines("Data/SeaLevel.txt");
        foreach (var l in a)
        {
            string[] s = l.Split('\t');
            if (short.Parse(s[0]) == Island)
            {
                TemporalSeaLevel = ushort.Parse(s[1]);
                break;
            }
        }
    }
    public static void ImportFile(string filename)
    {
        var fileBytes = System.IO.File.ReadAllBytes(filename);
        var headerBytes = new byte[4];
        Array.Copy(fileBytes, 0, headerBytes, 0, 4);

        var actualHeader = Encoding.UTF8.GetString(headerBytes);

        if (ExpectedFileHeader != actualHeader)
        {
            return;
        }

        _uncompressedBytes = new byte[fileBytes.Length - HeaderLength];

        Array.Copy(fileBytes, HeaderLength, _uncompressedBytes, 0, _uncompressedBytes.Length);
        Array.Copy(fileBytes, _headerBytes, HeaderLength);

        LoadFileCommon(filename);
    }

    private static void readExtra()
    {
        
        //extra
        var GratitudeBytes = new byte[4];
        GratitudeBytes[0] = _uncompressedBytes[0xC0FDC - HeaderLength];
        GratitudeBytes[1] = _uncompressedBytes[0xC0FDD - HeaderLength];
        GratitudeBytes[2] = _uncompressedBytes[0xC0FDE - HeaderLength];
        GratitudeBytes[3] = _uncompressedBytes[0xC0FDF - HeaderLength];
        GratitudePoints = BitConverter.ToUInt32(GratitudeBytes);

        GratitudeBytes[0] = _uncompressedBytes[0xC1060 - HeaderLength];
        GratitudeBytes[1] = _uncompressedBytes[0xC1061 - HeaderLength];
        GratitudeBytes[2] = _uncompressedBytes[0xC1062 - HeaderLength];
        GratitudeBytes[3] = _uncompressedBytes[0xC1063 - HeaderLength];
        Clock = BitConverter.ToSingle(GratitudeBytes);

        Weather = _uncompressedBytes[0xC1064 - HeaderLength];
    }

    private static void SaveFileExtra()
    {
        var chunkBytes = new byte[2];
        chunkBytes = BitConverter.GetBytes(ChunkCount);
        _uncompressedBytes[0x1451af] = chunkBytes[0];
        _uncompressedBytes[0x1451B0] = chunkBytes[1];
        chunkBytes = BitConverter.GetBytes(VirtualChunkCount);
        _uncompressedBytes[0x24e7c5] = chunkBytes[0];
        _uncompressedBytes[0x24e7c6] = chunkBytes[1];
        chunkBytes = BitConverter.GetBytes(VirtualItemCount);
        _uncompressedBytes[0x24e7cD] = chunkBytes[0];
        _uncompressedBytes[0x24e7cE] = chunkBytes[1];
    }
    public static void SaveFile()
    {
        byte[] compressedBytes;
        SaveFileExtra();

        using (var input = new MemoryStream(_uncompressedBytes))
        {
            using (var output = new MemoryStream())
            {
                using (var zlib = new System.IO.Compression.ZLibStream(output, System.IO.Compression.CompressionLevel.Optimal))
                {
                    input.CopyTo(zlib);
                    compressedBytes = output.ToArray();
                }
            }
        }
        var outputFileBytes = new byte[HeaderLength + compressedBytes.Length];

        Array.Copy(_headerBytes, outputFileBytes, HeaderLength);
        Array.Copy(compressedBytes, 0, outputFileBytes, HeaderLength, compressedBytes.Length);

        System.IO.File.WriteAllBytes(Filename, outputFileBytes);
    }
    public static void ExportFile(string filename)
    {
        SaveFileExtra();

        var outputFileBytes = new byte[HeaderLength + _uncompressedBytes.Length];

        Array.Copy(_headerBytes, outputFileBytes, HeaderLength);
        Array.Copy(_uncompressedBytes, 0, outputFileBytes, HeaderLength, _uncompressedBytes.Length);

        System.IO.File.WriteAllBytes(filename, outputFileBytes);
    }

    private static uint GetByteIndex(short chunk, byte layer, short tile)
    {
        return (uint)(BlockStart + (chunk * ChunkSize) + (layer * LayerSize) + (tile * 2));
    }

    public static ushort GetBlockValue(short chunk, byte layer, short tile)
    {
        var index = GetByteIndex(chunk, layer, tile);

        var blockBytes = new byte[2];
        blockBytes[0] = _uncompressedBytes[index];
        blockBytes[1] = _uncompressedBytes[index + 1];

        return BitConverter.ToUInt16(blockBytes);
    }

    public static void SetBlockValue(short chunk, byte layer, short tile, short blockId)
    {
        var index = GetByteIndex(chunk, layer, tile);

        _uncompressedBytes[index] = (byte)blockId;
        _uncompressedBytes[index + 1] = (byte)(blockId >> 8);
    }
    private static ItemInstance GetItemData(uint RelativeAddressID)
    {
        byte[] data = new byte[24];
        Array.Copy(_uncompressedBytes, RelativeAddressID * 24 + ItemStart, data, 0, 24);
        bool hasAllZeroes = data.All(singleByte => singleByte == 0);
        if (hasAllZeroes) return null;
        ItemInstance objectItem = new ItemInstance(data);
        objectItem.TileData = ObjectList.FirstOrDefault(t => t.Id == objectItem.Id) ?? ObjectList[0];
        return objectItem;
    }
    public static byte[] GetItemDataBytes(uint RelativeAddressID)
    {
        byte[] data = new byte[24];
        Array.Copy(_uncompressedBytes, RelativeAddressID * 24 + ItemStart, data, 0, 24);
        return data;
    }
    private static uint[] GetItemOffset(uint RelativeAddressID)
    {
        byte[] data = new byte[4];
        uint[] values = new uint[2];
        Array.Copy(_uncompressedBytes, RelativeAddressID * 4 + ItemOffsetStart, data, 0, 4);
        bool hasAllZeroes = data.All(singleByte => singleByte == 0);
        if (hasAllZeroes) return null;
        values[0] = (uint)(data[0] + ((data[1] & 0x0F) << 8)); 
        values[1] = (uint)(((data[1] & 0xF0) >> 4) + (data[2]<<4) + (data[3] << 12)); //defrag index
        return values;
    }

    public static void DeleteItem(ItemInstance Item)
    {
        var ItemOffset = GetItemOffset(Item.Offset);
        for(int ite=0;ite < 24; ite++) //delete Item
        {
            _uncompressedBytes[ItemOffset[1] * 24 + ItemStart + ite] = 0;
        }
        //Set Defrag area to 0
        _uncompressedBytes[Item.Offset * 4 + ItemOffsetStart + 1] =(byte)( _uncompressedBytes[Item.Offset * 4 + ItemOffsetStart + 1] & 0xF0);
        _uncompressedBytes[Item.Offset * 4 + ItemOffsetStart] = 0;
        TemporalItemCount--;
        //Update the registry of items per chunk. delete it.
        DeleteItemFromChunkItems(Item.Offset,Item.VirtualChunks());
        
    }

    private static void DeleteItemFromChunkItems(uint Offset,List<short> List)
    {
        foreach (var Chunk in List)
        {
            int ChunkI = (int)GetChunkFromGrid(Chunk);
            for (int i = 0; i < ChunkItems.ElementAt(ChunkI).Count(); i++)
                if (ChunkItems.ElementAt(ChunkI).ElementAt(i).Item1 == Offset)
                    ChunkItems.ElementAt(ChunkI).RemoveAt(i);
        }
    }

    public static void SetItem(ItemInstance Item, short chunk, byte layer, short tile)
    {
        uint i,itemOffset;
        byte[] DefragIndex = new byte[4];
        byte[] bytes = new byte[2];
        int x,z;
        x = tile % 32;
        z = tile / 32;
        for (i = 0; i < 0xC8000; i++) //Get the first defrag index that is empty
        {
            Array.Copy(_uncompressedBytes, i * 4 + ItemOffsetStart, DefragIndex, 0, 4);
            if ((DefragIndex[1] & 0x0F) + DefragIndex[0] == 0) break;
        }
        Item.ChunkGrid = (short)GetGridFromChunk(chunk);
        

        bytes = BitConverter.GetBytes(Item.ChunkGrid);
        DefragIndex[0] = bytes[0];
        DefragIndex[1] = (byte)(bytes[1] | DefragIndex[1]); //set chunk
        Array.Copy(DefragIndex, 0, _uncompressedBytes, i * 4 + ItemOffsetStart, 4);

        itemOffset = (uint)(((DefragIndex[1] & 0xF0) >> 4) + (DefragIndex[2] << 4) + (DefragIndex[3] << 12)); //defrag index
        
        Item.Offset = i;
        TemporalItemCount++;

        Item.Bytes[9] = (byte)(((x << 5)) | (Item.Bytes[9] & 0x1F)); //setting x,y,z
        Item.Bytes[10] = (byte)((x >> 3) + ((layer << 2) & 0xFF));
        Item.Bytes[11] = (byte)((layer >> 6) + (z << 1) | (Item.Bytes[9] & 0xC0));

        Array.Copy(Item.Bytes, 0, _uncompressedBytes, itemOffset * 24 + ItemStart, 24); //copying item
        foreach (var ch in Item.VirtualChunks())
            foreach (var Yc in Item.Ycoords())
                ChunkItems.ElementAt((int)GetChunkFromGrid(ch)).Add((Item.Offset, Yc));

    }
    public static void UpdateExtra()
    {
        if (Island == 0xFF) return;
        byte[] GratBytes = new byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(GratBytes, GratitudePoints);
        _uncompressedBytes[0xC0FDC - HeaderLength] = GratBytes[0];
        _uncompressedBytes[0xC0FDD - HeaderLength] = GratBytes[1];
        _uncompressedBytes[0xC0FDE - HeaderLength] = GratBytes[2];
        _uncompressedBytes[0xC0FDF - HeaderLength] = GratBytes[3];
        BinaryPrimitives.WriteSingleLittleEndian(GratBytes, Clock);
        _uncompressedBytes[0xC1060 - HeaderLength] = GratBytes[0];
        _uncompressedBytes[0xC1061 - HeaderLength] = GratBytes[1];
        _uncompressedBytes[0xC1062 - HeaderLength] = GratBytes[2];
        _uncompressedBytes[0xC1063 - HeaderLength] = GratBytes[3];
        _uncompressedBytes[0xC1064 - HeaderLength] = (byte)Weather;
    }
    public static List<ObservableProperty<SignText>> LoadSign()
    {
        List<ObservableProperty<SignText>> LineList = new List<ObservableProperty<SignText>>();
        var NameBytes = new byte[127];
        for (int i = 0; i < 20 * 134; i += 134)
        {
            Array.Copy(_uncompressedBytes, 0x14904 + i, NameBytes, 0, 127);
            if (_uncompressedBytes[0x14904 + i] != 0)
            {
                LineList.Add(new ObservableProperty<SignText>()
                {
                    Value = new SignText()
                    {
                        Size = 127,
                        Line = System.Text.Encoding.Default.GetString(NameBytes),
                        Index = i
                    }
                });
            }
        }
        var NameBytes2 = new byte[216];
        for (int i = 8761; i < 8761 + (60 * 228); i += 228)
        {
            Array.Copy(_uncompressedBytes, 0x14904 + i, NameBytes2, 0, 216);
            if (_uncompressedBytes[0x14904 + i] != 0)
            {
                LineList.Add(new ObservableProperty<SignText>()
                {
                    Value = new SignText()
                    {
                        Size = 216,
                        Line = System.Text.Encoding.Default.GetString(NameBytes2),
                        Index = i
                    }
                });
            }
        }
        return LineList;
    }
    public static void SaveSign(List<ObservableProperty<SignText>> LineList)
    {
        if (Island == 0xFF) return;
        var NameBytes = new byte[216];
        foreach (ObservableProperty<SignText> OLine in LineList)
        {
            var line = OLine.Value;
            var Bytes = new byte[line.Size];
            var BytesLen = new byte[line.Size];
            Bytes = Encoding.Default.GetBytes(line.Line);
            Array.Copy(Bytes, 0, BytesLen, 0, Bytes.Length);
            Array.Copy(BytesLen, 0, _uncompressedBytes, 0x14904 + line.Index, line.Size);
        }
    }
    public static void ReplaceBlockValueNew(ushort virtualChunk1, ushort virtualChunk2,
                                           ushort height1, ushort height2,
                                           ushort x1, ushort y1, ushort x2, ushort y2,
                                           List<short> blockOldId, short blockNewId){
        if (Island == 0xFF) return;
        ushort virtualChunk1x = (ushort)(virtualChunk1 % 64);
        ushort virtualChunk1y = (ushort)(virtualChunk1 / 64);
        ushort virtualChunk2x = (ushort)(virtualChunk2 % 64);
        ushort virtualChunk2y = (ushort)(virtualChunk2 / 64);
        ushort aux;
        ushort Cx1, Cy1, Cx2, Cy2;
        //flip if not in order
        if(height1 > height2) { aux = height1; height1 = height2; height2 = aux; }
        if (virtualChunk1x > virtualChunk2x) { aux = virtualChunk1x; virtualChunk1x = virtualChunk2x; virtualChunk2x = aux; }
        if (virtualChunk1y > virtualChunk2y) { aux = virtualChunk1y; virtualChunk1y = virtualChunk2y; virtualChunk2y = aux; }
        if (x1 > x2) { aux = x1; x1 = x2; x2 = aux; }
        if (y1 > y2) { aux = y1; y1 = y2; y2 = aux; }

        for (var Vx = virtualChunk1x; Vx <= virtualChunk2x; Vx++)
            for (var Vy = virtualChunk1y; Vy <= virtualChunk2y; Vy++)
            {
                if (Vx > virtualChunk1x) Cx1 = 0;
                else Cx1 = x1;
                if (Vx < virtualChunk2x) Cx2 = 31;
                else Cx2 = x2;
                if (Vy > virtualChunk1y) Cy1 = 0;
                else Cy1 = y1;
                if (Vy < virtualChunk2y) Cy2 = 31;
                else Cy2 = y2;
                if (GetChunkFromGrid(Vy * 64 + Vx) < VirtualChunkCount)
                    ReplaceBlockValueChunk(GetChunkFromGrid(Vy * 64 + Vx), height1, height2, Cx1, Cy1, Cx2, Cy2, blockOldId, blockNewId);
            }

    }
    public static void ReplaceBlockValueChunk(uint chunk,ushort layerBeg, ushort layerEnd,
                                           ushort x1, ushort y1, ushort x2, ushort y2,
                                           List<short> blockOldId, short blockNewId)
    {
        int index;
            for (byte layer = (byte)layerBeg; layer <= layerEnd; layer++)
            {
                for (var x = x1; x <= x2; x++)
                {
                    for (var y = y1; y <= y2; y++)
                    {
                        index = GetBlockValue((short)chunk, layer, (short)(y *32 + x));
                        if (blockOldId==null || blockOldId.Contains((short)(index % 2048)))
                        {
                            SetBlockValue((short)chunk, layer, (short)(y * 32 + x), blockNewId);
                        }
                    }
                }
            }
        }
    public static uint[] CountBlockData()
    {
        if (Island == 0xFF) return null;
        uint[] list = new uint[2048];
        short chunk = 0;
        for (int i = 0; i < 2048; i++)
        {
            list[i] = 0;
        }
        while (chunk < VirtualChunkCount)
        {
            for (byte layer = 0x00; layer < 0x60; layer += 0x01)
                for (short tile = 0; tile < 1024; tile++)
                {
                    var a = GetBlockValue(chunk, layer, tile) % 2048;
                    list[a] = list[a] + 1;
                }
            chunk++;
        }
        return list;
    }
    public static void CountItemData(StreamWriter file,List<TileData> TileListObject)
    {
        if (Island == 0xFF) return;
        for (uint i = 0; i < 0xC8000; i++)
        {
            var j = GetItemOffset(i); //0 CHUNK, 1 OFFSET
            if (j != null)
            {
                var a = GetItemData(j[1]);
                if (a != null)
                {
                    a.ChunkGrid = (short)j[0];
                    a.Offset = i;
                    if (a.Id > 1)
                    {
                        file.WriteLine("ID: " + a.Id + " " + (TileListObject.FirstOrDefault(t => t.Id == a.Id) ?? TileListObject[0]).Name + " " + a.Chunk + " " + a.PosX + " " + a.PosY + " " + a.PosZ);
                    }
                }
            }
        }
        return;
    }
    private static void ReadAllItems()
    {
        TemporalItemCount = 0;
        ChunkItems = new List<List<(uint,ushort)>>();
        //Store all items via the chunks they are in. This saves the pointer to the offset
        for (int i = 0; i < VirtualChunkCount; i++) {
            ChunkItems.Add(new List<(uint, ushort)>());
        }
        for (uint i = 0; i < 0xC8000; i++)
        {
            var j = GetItemOffset(i);//0 CHUNK, 1 OFFSET

            if (j != null && j[0] != 0)
            {
                var a = GetItemData(j[1]);
                TemporalItemCount++;
                a.ChunkGrid = (short)j[0];

                foreach(var ch in a.VirtualChunks())
                    foreach (var Yc in a.Ycoords())
                        ChunkItems.ElementAt((int)GetChunkFromGrid(ch)).Add((i, Yc));
            }
        }
    }

    public static List<ItemInstance> ReadItemsChunkLayer(short chunk, byte layer)
    {
        if (Island == 0xFF) return null;
        var list = new List<ItemInstance>();
        foreach(var i in ChunkItems.ElementAt(chunk))
        {
            if (i.Item2 == layer)
            {
                var a = GetItemData(GetItemOffset(i.Item1)[1]);
                if (a != null)
                {
                    if (a.Ycoords().Contains(layer))
                    {
                        a.Offset = i.Item1;
                        a.ChunkGrid = (short)GetGridFromChunk(chunk);
                        list.Add(a);
                    }
                }
            }
        }
        return list;
    }
    public static uint GetChunkFromGrid(int index)
    {
        byte[] data = new byte[2];
        Array.Copy(_uncompressedBytes, index * 2 + ChunkGridStart, data, 0, 2);
        var Offset = BitConverter.ToUInt16(data);
        return Offset;
    }

    public static uint GetGridFromChunk(int chunk)
    {
        byte[] data = new byte[2];
        for (uint i = 0; i < ChunkGridSize/2; i++)
        {
            Array.Copy(_uncompressedBytes, i * 2 + ChunkGridStart, data, 0, 2);
            var ChunkRead = BitConverter.ToUInt16(data);
            if(ChunkRead == chunk)
            {
                return i;
            }
        }
        return 0;
    }
    public static void SetChunkFromGrid(int index, uint value)
    {
        byte[] data = new byte[2];
        data = BitConverter.GetBytes(value);
        Array.Copy(data, 0, _uncompressedBytes, index * 2 + ChunkGridStart, 2);
    }
    public static void ChunkGridEdit( List<uint> ChunkGridList, List<uint> ChunkGridListOld, short ChunkCount)
    {
        byte[] ChunkBytes = new byte[ChunkCount * ChunkSize];
        byte[] NewUncompressedBytes = new byte[BlockStart + ChunkCount * ChunkSize];
        for (int i=0; i < ChunkGridList.Count; i++)
        {
            SetChunkFromGrid(i, ChunkGridList[i]);
            if (ChunkGridList[i] != 0xFFFF)
            {
                if(ChunkGridListOld[i] != 0xFFFF)
                { //copy the old chunk reference to the new chunk reference
                    Array.Copy(_uncompressedBytes, BlockStart + (ChunkGridListOld[i] * ChunkSize), ChunkBytes, (ChunkGridList[i] * ChunkSize), ChunkSize);
                }
                else
                {
                    for(int j = 0; j < LayerSize; j +=2)
                    {
                        ChunkBytes[ChunkGridList[i] * ChunkSize + j] = 1; //set bedrock
                    }
                }
            }
        }
        Array.Copy(_uncompressedBytes, NewUncompressedBytes, BlockStart);
        Array.Copy(ChunkBytes,0, NewUncompressedBytes, BlockStart, ChunkCount * ChunkSize);
        _uncompressedBytes = NewUncompressedBytes;
        ReadAllItems(); //Chunks have changed.
    }
    public static uint[] GetChunkCrop()
    {
        uint[] uints = new uint[4];
        uints[0] = 99999;
        uints[2] = 99999;
        for (uint i = 1; i < ChunkGridSize; i += 2)
        {
            if(_uncompressedBytes != null && _uncompressedBytes[ChunkGridStart + i] != 0xFF)
            {
                if ((i/128) < uints[0]) uints[0] = (i / 128);
                if ((i/128) > uints[1]) uints[1] = (i / 128);
                if ((i / 2 % 64) < uints[2]) uints[2] = (i/2 % 64);
                if ((i / 2 % 64) > uints[3]) uints[3] = (i/2 % 64);
            }
        }
        return uints;
    }
    public static void KILLITEMS()
    {
        byte[] data = new byte[24];
        for (uint i = 0; i < 24; i++)
        {
            data[i] = 0;
        }
        byte[] offset = new byte[4];
        for (uint i = 0; i < 4; i++)
        {
            data[i] = 0;
        }
        for (uint i = 2; i < 0xC8000; i++)
        {
            Array.Copy(data, 0,_uncompressedBytes, i * 24 + ItemStart, 24);
            Array.Copy(offset, 0, _uncompressedBytes, i * 4 + ItemOffsetStart, 4);
        }
    }
}
