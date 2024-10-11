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


namespace DQB2ChunkEditor;

public static class ChunkEditor
{
    private const uint LayerSize = 0x800; // layers are 32x32 * 2
    private const uint HeaderLength = 0x110; // everything after the header is compressed data
    private const uint ChunkSize = 0x30000; // chunk size is layer size (1024) * layer height (96) * block size (2)
    private const uint BlockStart = 0x183FEF0; // starting index for the block data
    private const uint ItemStart = 0x24E7D1; // starting index for the block data
    private const string ExpectedFileHeader = "aerC"; // expected file header for the map

    private static byte[] _headerBytes = new byte[HeaderLength];
    private static byte[] _uncompressedBytes = null!;

    public static string Filename { get; set; } = null!;
    public const uint LayerHeight = 0x60; // layers are up to 96 blocks high

    public static string Pname { get; set; } = null!;
    public static UInt32 GratitudePoints { get; set; } = 99999; //gratitude points
    public static float Clock { get; set; } = 0; //clock
    public static ushort Weather { get; set; } = 1; //weather
    public static short ChunkCount { get; set; } = 0; // chunk count changes per map
    public static byte Island { get; set; } = 0xFF; //Island Number

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
        chunkBytes[0] = _uncompressedBytes[1331903 - HeaderLength];
        chunkBytes[1] = _uncompressedBytes[1331904 - HeaderLength];
        ChunkCount = (short)(BitConverter.ToUInt16(chunkBytes));
        Filename = filename;
        readExtra();
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

        var chunkBytes = new byte[2];
        chunkBytes[0] = _uncompressedBytes[1331903 - HeaderLength];
        chunkBytes[1] = _uncompressedBytes[1331904 - HeaderLength];
        ChunkCount = (short)(BitConverter.ToUInt16(chunkBytes));
        Filename = filename;
        readExtra();
    }

    private static void readExtra()
    {
        Island = _uncompressedBytes[0xC0FE6 - HeaderLength];
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
    private static int GetItemID(uint RelativeAddressID)
    {
        byte[] data = new byte[24];
        Array.Copy(_uncompressedBytes, RelativeAddressID * 24 + ItemStart, data, 0, 24);
        bool hasAllZeroes = data.All(singleByte => singleByte == 0);
        if (hasAllZeroes) return -1;
        int ID = data[8] + ((data[9] & 0xF0) >> 8);
        return ID;
    }
    private static void SetItemID(uint RelativeAddressID, short newId)
    {
        byte[] data = new byte[2];
        data = BitConverter.GetBytes(newId);
        _uncompressedBytes[RelativeAddressID * 24 + ItemStart + 8] = data[0];
        _uncompressedBytes[RelativeAddressID * 24 + ItemStart + 9] = data[1];
        //_uncompressedBytes[RelativeAddressID * 24 + ItemStart + 8] = (byte)(data[0]
        //    | (_uncompressedBytes[RelativeAddressID * 24 + ItemStart + 8] & 0xF0));
        //_uncompressedBytes[RelativeAddressID * 24 + ItemStart + 9] = data[1];
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

    public static void ReplaceBlockValue(short chunkBeg, short chunkEnd,
                                           short layerBeg, short layerEnd, 
                                           short tileBeg, short tileEnd,
                                           List<short> blockOldId, short blockNewId)
    {
        if (Island == 0xFF) return;
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
        if (Island == 0xFF) return null;
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
    public static uint[] CountItemData()
    {
        if (Island == 0xFF) return null;
        uint[] list = new uint[17327];
        for (int i = 0; i < 17326; i++)
        {
            list[i] = 0;
        }
        for (uint i = 0; i < 0xC8000; i++)
        {
            var a = GetItemID(i);
            if (a > 17326 || a < 0)
            {
                list[17326] = (uint)a;
            }
            else
            {
                list[a] = list[a] + 1;
            }
        }
        return list;
    }
}
