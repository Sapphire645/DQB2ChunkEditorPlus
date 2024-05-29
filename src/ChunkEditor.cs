using System;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography.Xml;
using System.Text;
using Ionic.Zlib;

namespace DQB2ChunkEditor;

public static class ChunkEditor
{
    private const uint LayerSize = 0x800; // layers are 32x32 * 2
    private const uint HeaderLength = 0x110; // everything after the header is compressed data
    private const uint ChunkSize = 0x30000; // chunk size is layer size (1024) * layer height (96) * block size (2)
    private const uint BlockStart = 0x183FEF0; // starting index for the block data
    private const string ExpectedFileHeader = "aerC"; // expected file header for the map

    private static byte[] _headerBytes = new byte[HeaderLength];
    private static byte[] _uncompressedBytes = null!;

    public static string Filename { get; set; } = null!;
    public const uint LayerHeight = 0x60; // layers are up to 96 blocks high
    public static short ChunkCount { get; set; } = 0; // chunk count changes per map

    public static void LoadFile(string filename)
    {
        var fileBytes = File.ReadAllBytes(filename);
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

        _uncompressedBytes = ZlibStream.UncompressBuffer(compressedBytes);

        var chunkBytes = new byte[2];
        chunkBytes[0] = _uncompressedBytes[1331903-HeaderLength];
        chunkBytes[1] = _uncompressedBytes[1331904-HeaderLength];
        ChunkCount = (short)(BitConverter.ToUInt16(chunkBytes));
        Filename = filename;
    }

    public static void SaveFile()
    {
        var compressedBytes = ZlibStream.CompressBuffer(_uncompressedBytes);
        var outputFileBytes = new byte[HeaderLength + compressedBytes.Length];

        Array.Copy(_headerBytes, outputFileBytes, HeaderLength);
        Array.Copy(compressedBytes, 0, outputFileBytes, HeaderLength, compressedBytes.Length);

        File.WriteAllBytes(Filename, outputFileBytes);
    }

    public static void ExportFile(string filename)
    {
        var outputFileBytes = new byte[HeaderLength + _uncompressedBytes.Length];

        Array.Copy(_headerBytes, outputFileBytes, HeaderLength);
        Array.Copy(_uncompressedBytes, 0, outputFileBytes, HeaderLength, _uncompressedBytes.Length);

        File.WriteAllBytes(filename, outputFileBytes);
    }

    public static void ImportFile(string filename)
    {
        var fileBytes = File.ReadAllBytes(filename);
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

        var compressedBytes = ZlibStream.CompressBuffer(_uncompressedBytes);

        ChunkCount = (short)(_uncompressedBytes.Length / ChunkSize);
        Filename = filename;
    }
    private static uint GetByteIndex(short chunk, byte layer, short tile)
    {
        return (uint)(BlockStart + (chunk * ChunkSize) + (layer * LayerSize) + (tile * 2));
    }

    //private static uint GetItemIndex(short chunk, byte layer, short tile)
    //{
    //    return (uint)(BlockStart + (chunk * ChunkSize) + (layer * LayerSize) + (tile * 2));
    //}
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

        _uncompressedBytes[index] = (byte)(blockId % 2048);
        _uncompressedBytes[index + 1] = (byte)(blockId >> 8);
    }
    public static void SetAreaValue(short chunk, byte layer, short tileBeg, short tileEnd, short blockId)
    {
        short aux;
        if (tileBeg > tileEnd){
            aux = tileBeg;
            tileBeg = tileEnd;
            tileEnd = aux;
        }
        if ((tileBeg%32)>(tileEnd%32)){
            aux = tileBeg;
            tileBeg = (short)(tileBeg-(tileBeg%32)+(tileEnd%32));
            tileEnd = (short)(tileEnd+(aux%32)-(tileEnd%32));
        }
        var x0 = (tileBeg%32);
        var x1 = (tileEnd%32);
        for(var TileOrd=tileBeg; TileOrd<= tileEnd; TileOrd++){
            if(TileOrd%32 >= x0 && TileOrd%32 <=x1){
                SetBlockValue(chunk, layer, TileOrd, blockId);
            }
        }
    }

    public static void ReplaceBlockValue(short blockOldId, short blockNewId)
    {
        short chunk = 0;
        uint index = 0;
        while(chunk < ChunkCount){
            for(byte layer = 0x00; layer < 0x60; layer += 0x01){
                for(short tile = 0; tile < 1024; tile ++){
                    index = GetBlockValue(chunk, layer, tile);
                    if(index%2048 == blockOldId){
                        SetBlockValue(chunk, layer, tile, blockNewId);
                    }
                }
            }
            chunk++;
                
        }
    }
    public static void ReplaceAreaValue(short chunk, byte layer, short tileBeg, short tileEnd, short blockOldId, short blockNewId)
    {
        uint index;
        short aux;
        if (tileBeg > tileEnd){
            aux = tileBeg;
            tileBeg = tileEnd;
            tileEnd = aux;
        }
        if ((tileBeg%32)>(tileEnd%32)){
            aux = tileBeg;
            tileBeg = (short)(tileBeg-(tileBeg%32)+(tileEnd%32));
            tileEnd = (short)(tileEnd+(aux%32)-(tileEnd%32));
        }
        var x0 = (tileBeg%32);
        var x1 = (tileEnd%32);
        for(var TileOrd=tileBeg; TileOrd<= tileEnd; TileOrd++){
            if(TileOrd%32 >= x0 && TileOrd%32 <=x1){
                index = GetBlockValue(chunk, layer, TileOrd);
                    if(index%2048 == blockOldId){
                        SetBlockValue(chunk, layer, TileOrd, blockNewId);
                    }
            }
        }
    }

    public static void Flatten()
    {
        short chunk = 0;
        while(chunk < ChunkCount){
            for(short tile = 0; tile < 1024; tile ++)
                    SetBlockValue(chunk, 0x00, tile, 1);
            for(byte layer = 0x01; layer < 0x60; layer += 0x01)
                for(short tile = 0; tile < 1024; tile ++)
                    SetBlockValue(chunk, layer, tile, 0);
            
            chunk++;
            }
                
    }

    //code by Turtle_insect

    // public static void GetItemValue(short chunk, byte layer, short tile, short blockId)
    // {
    //     var address = GetItemIndex(short chunk, byte layer, short tile);

    //     ItemID = SaveData.Instance().ReadNumber(address + 8, 1);
    //     var tmp = SaveData.Instance().ReadNumber(address + 9, 1) & 0x1F; //mask
    //     ItemID += tmp * 256;

    //     PosX = SaveData.Instance().ReadNumber(address + 9, 1) >> 5;
    //     tmp = SaveData.Instance().ReadNumber(address + 10, 1) & 0x3; //mask
    //     PosX += tmp << 3;

    //     PosY = SaveData.Instance().ReadNumber(address + 10, 1) >> 2;
    //     tmp = SaveData.Instance().ReadNumber(address + 11, 1) & 0x1; //mask
    //     PosY += tmp << 6;

    //     PosZ = (SaveData.Instance().ReadNumber(address + 11, 1) & 0x3E);
    //     PosZ >>= 1;
    // }
}
