﻿using System;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Windows.Markup;
using System.Buffers.Binary;
using Ionic.Zlib;
using System.Windows.Documents;
using System.Collections.Generic;
using DQB2ChunkEditor.Windows;
using DQB2ChunkEditor.Models;
using System.Collections.ObjectModel;
using System.Windows.Shapes;
using System.Drawing;
using System.Collections;

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

    public static string Pname { get; set; } = null!;
    public const uint LayerHeight = 0x60; // layers are up to 96 blocks high
    public static short ChunkCount { get; set; } = 0; // chunk count changes per map
    public static short ItemCount { get; set; } = 0; // chunk count changes per map

    public static UInt32 GratitudePoints{ get; set; } = 99999; //gratitude points
    public static float Clock { get; set; } = 0; //clock
    public static ushort Weather{ get; set; } = 1; //weather

    public static byte Island{ get; set; } = 0xFF; //Island Number

    public static void LoadFile(string filename)
    {
        var fileBytes = File.ReadAllBytes(filename);
        var headerBytes = new byte[4];
        Array.Copy(fileBytes, 0, headerBytes, 0, 4);
        var pnameBytes = new byte[28];
        Array.Copy(fileBytes, 0xCD, pnameBytes,0, 28);
        Pname = Encoding.UTF8.GetString(pnameBytes);

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

        chunkBytes[0] = _uncompressedBytes[0x24E7CD];
        chunkBytes[1] = _uncompressedBytes[0x24E7CE];
        ItemCount = (short)(BitConverter.ToUInt16(chunkBytes));
        Filename = filename;
        Island = _uncompressedBytes[0xC0FE6-HeaderLength];
        //extra
        var GratitudeBytes = new byte[4];
        GratitudeBytes[0] = _uncompressedBytes[0xC0FDC-HeaderLength];
        GratitudeBytes[1] = _uncompressedBytes[0xC0FDD-HeaderLength];
        GratitudeBytes[2] = _uncompressedBytes[0xC0FDE-HeaderLength];
        GratitudeBytes[3] = _uncompressedBytes[0xC0FDF-HeaderLength];
        GratitudePoints = BitConverter.ToUInt32(GratitudeBytes);

        GratitudeBytes[0] = _uncompressedBytes[0xC1060-HeaderLength];
        GratitudeBytes[1] = _uncompressedBytes[0xC1061-HeaderLength];
        GratitudeBytes[2] = _uncompressedBytes[0xC1062-HeaderLength];
        GratitudeBytes[3] = _uncompressedBytes[0xC1063-HeaderLength];
        Clock = BitConverter.ToSingle(GratitudeBytes);

        Weather = _uncompressedBytes[0xC1064-HeaderLength];
    }
    public static ObservableCollection<SignText> LoadSign()
    {
        ObservableCollection<SignText> LineList = new ObservableCollection<SignText>();
        var NameBytes = new byte[127];
        for (int i = 0; i < 20*134; i += 134)
        {
            Array.Copy(_uncompressedBytes, 0x14904 + i, NameBytes, 0, 127);
            if (_uncompressedBytes[0x14904 + i] != 0)
            {
                LineList.Add(new SignText()
                {
                    Size = 127,
                    Line = System.Text.Encoding.Default.GetString(NameBytes),
                    Index = i

                });
            }
        }
        var NameBytes2 = new byte[216];
        for (int i = 8761; i < 8761 + (60 * 228); i += 228)
        {
            Array.Copy(_uncompressedBytes, 0x14904 + i, NameBytes2, 0, 216);
            if (_uncompressedBytes[0x14904 + i] != 0)
            {
                LineList.Add(new SignText()
                {
                    Size = 216,
                    Line = System.Text.Encoding.Default.GetString(NameBytes2),
                    Index = i

                });
            }
        }
        return LineList;
    }
    public static void SaveSign(ObservableCollection<SignText> LineList)
    {
        var NameBytes = new byte[216];
        foreach( SignText line in LineList )
        {
            var Bytes = new byte[line.Size];
            var BytesLen = new byte[line.Size];
            Bytes = Encoding.Default.GetBytes(line.Line);
            Array.Copy(Bytes, 0, BytesLen, 0, Bytes.Length);
            Array.Copy(BytesLen, 0, _uncompressedBytes, 0x14904 + line.Index, line.Size);
        }
    }
    public static void SaveSignColorFind(ObservableCollection<SignText> LineList)
    {
        int number = 890;
        var Line = "<$cdef(" + number + ")>♥" + number + "</color><$cdef(" + (number + 1) + ")>♥" + (number + 1) + "</color><$cdef(" + (number + 2) + ")>♥" + (number + 2) + "</color>";
        foreach (SignText line in LineList)
        {
            var Bytes = new byte[line.Size];
            var BytesLen = new byte[line.Size];
            if (line.Size < 200)
            {
                number = number + 3;
                Line = "<$cdef(" + number + ")>♥" + number + "</color><$cdef(" + (number + 1) + ")>♥" + (number + 1) + "</color><$cdef(" + (number + 2) + ")>♥" + (number + 2) + "</color>";
            }
            else
            {
                number = number + 3;
                Line = "<$cdef(" + number + ")>♥" + number + "</color><$cdef(" + (number + 1) + ")>♥" + (number + 1) + "</color><$cdef(" + (number + 2) + ")>♥" + (number + 2) + "</color>";
                number = number + 3;
                Line += "<$cdef(" + number + ")>♥" + number + "</color><$cdef(" + (number + 1) + ")>♥" + (number + 1) + "</color><$cdef(" + (number + 2) + ")>♥" + (number + 2) + "</color>";
            }
            Bytes = Encoding.Default.GetBytes(Line);
            Array.Copy(Bytes, 0, BytesLen, 0, Bytes.Length);
            Array.Copy(BytesLen, 0, _uncompressedBytes, 0x14904 + line.Index, line.Size);
        }
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

        var chunkBytes = new byte[2];
        chunkBytes[0] = _uncompressedBytes[1331903-HeaderLength];
        chunkBytes[1] = _uncompressedBytes[1331904-HeaderLength];
        ChunkCount = (short)(BitConverter.ToUInt16(chunkBytes));

        Filename = filename;

        Island = _uncompressedBytes[0xC0FE6-HeaderLength];
        //extra
        var GratitudeBytes = new byte[4];
        GratitudeBytes[0] = _uncompressedBytes[0xC0FDC-HeaderLength];
        GratitudeBytes[1] = _uncompressedBytes[0xC0FDD-HeaderLength];
        GratitudeBytes[2] = _uncompressedBytes[0xC0FDE-HeaderLength];
        GratitudeBytes[3] = _uncompressedBytes[0xC0FDF-HeaderLength];
        GratitudePoints = BitConverter.ToUInt32(GratitudeBytes);
        
        GratitudeBytes[0] = _uncompressedBytes[0xC1060-HeaderLength];
        GratitudeBytes[1] = _uncompressedBytes[0xC1061-HeaderLength];
        GratitudeBytes[2] = _uncompressedBytes[0xC1062-HeaderLength];
        GratitudeBytes[3] = _uncompressedBytes[0xC1063-HeaderLength];
        Clock = BitConverter.ToSingle(GratitudeBytes);

        Weather = _uncompressedBytes[0xC1064-HeaderLength];
    }
    private static uint GetByteIndex(short chunk, byte layer, short tile)
    {
        return (uint)(BlockStart + (chunk * ChunkSize) + (layer * LayerSize) + (tile * 2));
    }

    public static uint GetItemData(uint address)
    {
        return _uncompressedBytes[address];
    }
    

    public static uint GetItemChunk(uint address)
    {
        var blockBytes = new byte[2];
        blockBytes[0] = _uncompressedBytes[address];
        blockBytes[1] = _uncompressedBytes[address + 1];

        return BitConverter.ToUInt16(blockBytes);
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
    public static void KillLiquid(short chunkBeg, short chunkEnd, byte layerBeg, short layerEnd, List<short> blockId)
    {
        short chunk = chunkBeg;
        uint index = 0;
        while (chunk < chunkEnd)
        {
            for (byte layer = layerBeg; layer < layerEnd; layer += 0x01)
            {
                for (short tile = 0; tile < 1024; tile++)
                {
                    index = GetBlockValue(chunk, layer, tile);
                    if (blockId.Contains((short)(index % 2048)))
                    {
                        SetBlockValue(chunk, layer, tile, 0);
                    }
                }
            }
            chunk++;
        }
    }
    public static void ReplaceBlockValue(short chunkBeg, short chunkEnd, byte layerBeg, short layerEnd, short blockOldId, short blockNewId)
    {
        short chunk = chunkBeg;
        uint index = 0;
        while(chunk < chunkEnd)
        {
            for(byte layer = layerBeg; layer < layerEnd; layer += 0x01){
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

    public static void MassSetBlockValue(short chunkBeg, short chunkEnd, byte layerBeg, short layerEnd, short blockNewId)
    {
        short chunk = chunkBeg;
        while (chunk <= chunkEnd)
        {
            for (byte layer = layerBeg; layer <= layerEnd; layer += 0x01)
            {
                for (short tile = 0; tile < 1024; tile++)
                {
                    SetBlockValue(chunk, layer, tile, blockNewId);
                }
            }
            chunk++;

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
    public static uint[] CountBlockData()
    {
        uint[] list = new uint[2048];
        short chunk = 0;
        for(int i=0; i < 2048; i++)
        {
            list[i] = 0;
        }
        while (chunk < ChunkCount)
        {
            for (byte layer = 0x00; layer < 0x60; layer += 0x01)
                for (short tile = 0; tile < 1024; tile++)
                {
                    var a = GetBlockValue(chunk, layer, tile)%2048;
                    list[a] = list[a] + 1;
                }
            chunk++;
        }
        return list;
    }
    public static void UpdateExtra(){
        byte[] GratBytes = new byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(GratBytes,GratitudePoints);
        _uncompressedBytes[0xC0FDC-HeaderLength] = GratBytes[0];
        _uncompressedBytes[0xC0FDD-HeaderLength] = GratBytes[1];
        _uncompressedBytes[0xC0FDE-HeaderLength] = GratBytes[2];
        _uncompressedBytes[0xC0FDF-HeaderLength] = GratBytes[3];
        BinaryPrimitives.WriteSingleLittleEndian(GratBytes,Clock);
        _uncompressedBytes[0xC1060-HeaderLength] = GratBytes[0];
        _uncompressedBytes[0xC1061-HeaderLength] = GratBytes[1];
        _uncompressedBytes[0xC1062-HeaderLength] = GratBytes[2];
        _uncompressedBytes[0xC1063-HeaderLength] = GratBytes[3];
        _uncompressedBytes[0xC1064-HeaderLength] = (byte)Weather;
    }
}
