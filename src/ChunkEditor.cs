using System;
using System.Buffers.Binary;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using DQBChunkEditor.Models;
using DQBChunkEditor.Windows;
using Ionic.Zlib;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Reflection;


namespace DQBChunkEditor;

public static class ChunkEditor
{
    private const uint LayerSize = 0x400; // layers are 32x32
    private const uint ChunkSize = 0x8000; // chunk size is layer size (1024) * layer height (32)

    private const uint ChunkGridSize = 0x1200;
    private const uint ItemSize = 0;
    private const uint ItemOffsetSize = 0;

    private const uint ChunkGridStart = 0x0; // starting index for the chunk offsets
    private const uint ItemStart = 0x0; // starting index for the item data
    private const uint ItemOffsetStart = 0x0; // starting index for the chunk offsets
    private const uint BlockStart = 0x120c; // starting index for the block data

    private static byte[] _uncompressedBytes = null!;

    public static string Filename { get; set; } = null!;
    public const uint LayerHeight = 32; // layers are up to 32 blocks high

    public static short ChunkCount { get; set; } = 0; // chunk count changes per map

    public static void LoadFile(string filename)
    {
        var fileBytes = System.IO.File.ReadAllBytes(filename);
        var headerBytes = new byte[4];

        using (var input = new MemoryStream(fileBytes))
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
        //_uncompressedBytes = ZlibStream.UncompressBuffer(fileBytes);

        var chunkBytes = new byte[2];
        chunkBytes[0] = _uncompressedBytes[0x1204];
        chunkBytes[1] = _uncompressedBytes[0x1205];
        ChunkCount = (short)(BitConverter.ToUInt16(chunkBytes));
        Filename = filename;
    }
    //public static void ImportFile(string filename)
    //{
    //    var fileBytes = System.IO.File.ReadAllBytes(filename);
    //    var headerBytes = new byte[4];
    //    Array.Copy(fileBytes, 0, headerBytes, 0, 4);

    //    var actualHeader = Encoding.UTF8.GetString(headerBytes);

    //    if (ExpectedFileHeader != actualHeader)
    //    {
    //        return;
    //    }

    //    _uncompressedBytes = new byte[fileBytes.Length - HeaderLength];

    //    Array.Copy(fileBytes, HeaderLength, _uncompressedBytes, 0, _uncompressedBytes.Length);
    //    Array.Copy(fileBytes, _headerBytes, HeaderLength);

    //    var compressedBytes = ZlibStream.CompressBuffer(_uncompressedBytes);

    //    var chunkBytes = new byte[2];
    //    chunkBytes[0] = _uncompressedBytes[1331903 - HeaderLength];
    //    chunkBytes[1] = _uncompressedBytes[1331904 - HeaderLength];
    //    ChunkCount = (short)(BitConverter.ToUInt16(chunkBytes));
    //    Filename = filename;
    //    readExtra();
    //}

    public static void SaveFile()
    {
        var chunkBytes = new byte[2];
        chunkBytes = BitConverter.GetBytes(ChunkCount);
        _uncompressedBytes[0x1204] = chunkBytes[0];
        _uncompressedBytes[0x1205] = chunkBytes[1];

        using (var input = new MemoryStream(_uncompressedBytes))
        {
            using (var output = new MemoryStream())
            {
                using (var zlib = new System.IO.Compression.ZLibStream(output, System.IO.Compression.CompressionLevel.Fastest))
                {
                    input.CopyTo(zlib);
                }
                System.IO.File.WriteAllBytes(Filename, output.ToArray());
            }
        }
    }
    //public static void ExportFile(string filename)
    //{
    //    var chunkBytes = new byte[2];
    //    chunkBytes = BitConverter.GetBytes(ChunkCount);
    //    _uncompressedBytes[1331903 - HeaderLength] = chunkBytes[0];
    //    _uncompressedBytes[1331904 - HeaderLength] = chunkBytes[1];

    //    var outputFileBytes = new byte[HeaderLength + _uncompressedBytes.Length];

    //    Array.Copy(_headerBytes, outputFileBytes, HeaderLength);
    //    Array.Copy(_uncompressedBytes, 0, outputFileBytes, HeaderLength, _uncompressedBytes.Length);

    //    System.IO.File.WriteAllBytes(filename, outputFileBytes);
    //}

    private static uint GetByteIndex(short chunk, byte layer, short tile)
    {
        return (uint)(BlockStart + (chunk * ChunkSize) + (layer * LayerSize) + (tile));
    }

    public static ushort GetBlockValue(short chunk, byte layer, short tile)
    {
        var index = GetByteIndex(chunk, layer, tile);
        return _uncompressedBytes[index];
    }

    public static void SetBlockValue(short chunk, byte layer, short tile, short blockId)
    {
        var index = GetByteIndex(chunk, layer, tile);

        _uncompressedBytes[index] = (byte)blockId;
    }

   
    public static void ReplaceBlockValue(short chunkBeg, short chunkEnd,
                                           short layerBeg, short layerEnd, 
                                           short tileBeg, short tileEnd,
                                           List<short> blockOldId, short blockNewId)
    {
        short chunk = chunkBeg;
        uint index = 0;
        short aux;
        if (tileBeg > tileEnd)
        {
            aux = tileBeg;
            tileBeg = tileEnd;
            tileEnd = aux;
        }
        if ((tileBeg % 32) > (tileEnd % 32))
        {
            aux = tileBeg;
            tileBeg = (short)(tileBeg - (tileBeg % 32) + (tileEnd % 32));
            tileEnd = (short)(tileEnd + (aux % 32) - (tileEnd % 32));
        }
        var x0 = (tileBeg % 32);
        var x1 = (tileEnd % 32);
        while (chunk <= chunkEnd)
        {
            for (byte layer = (byte)layerBeg; layer <= layerEnd; layer += 0x01)
            {
                for (var TileOrd = tileBeg; TileOrd <= tileEnd; TileOrd++)
                {
                    if (TileOrd % 32 >= x0 && TileOrd % 32 <= x1)
                    {
                        index = GetBlockValue(chunk, layer, TileOrd);
                        if (blockOldId==null || blockOldId.Contains((short)(index % 2048)))
                        {
                            SetBlockValue(chunk, layer, TileOrd, blockNewId);
                        }
                    }
                }
            }
            chunk++;
        }
    }
    public static uint[] CountBlockData()
    {
        uint[] list = new uint[2048];
        short chunk = 0;
        for (int i = 0; i < 2048; i++)
        {
            list[i] = 0;
        }
        while (chunk < ChunkCount)
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
    }
    public static uint[] GetChunkCrop()
    {
        uint[] uints = new uint[4];
        uints[0] = 99999;
        uints[2] = 99999;
        for (uint i = 1; i < ChunkGridSize; i += 2)
        {
            if(_uncompressedBytes[ChunkGridStart + i] != 0xFF)
            {
                if ((i/128) < uints[0]) uints[0] = (i / 128);
                if ((i/128) > uints[1]) uints[1] = (i / 128);
                if ((i / 2 % 64) < uints[2]) uints[2] = (i/2 % 64);
                if ((i / 2 % 64) > uints[3]) uints[3] = (i/2 % 64);
            }
        }
        return uints;
    }
}
