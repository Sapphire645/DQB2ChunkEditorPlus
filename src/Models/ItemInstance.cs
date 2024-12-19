using System;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace DQB2ChunkEditor.Models
{
	public class ItemInstance
	{
        public short Id { get; set; } = -1;
		public uint Offset { get; set; }
        public short ChunkGrid { get; set; }
		public short Chunk  => (short)ChunkEditor.GetChunkFromGrid(ChunkGrid);
        public byte PosX { get; set; }
        public byte PosY { get; set; }
        public byte PosZ { get; set; }
		public byte Rotation { get; set; }
        public short ChunkRow => (short)(ChunkGrid / 64);
        public short ChunkCol => (short)(ChunkGrid % 64);
		public byte[] Bytes { get; set; }
		public TileData TileData { get; set; }

		public List<short> VirtualChunks()
		{
			var List = new List<short>();
			List.Add(ChunkGrid);
			if (PosX + TileData.ObjectData.X > 32) List.Add((short)(ChunkGrid + 1));
			if (PosZ + TileData.ObjectData.Z > 32) List.Add((short)(ChunkGrid + 64));
            if (PosZ + TileData.ObjectData.Z > 32 && PosX + TileData.ObjectData.X > 32) List.Add((short)(ChunkGrid + 65));
			return List;
        }
        public List<byte> Ycoords()
        {
            var List = new List<byte>();
            for(byte i = 0; i < TileData.ObjectData.Y; i++)
			{
                List.Add((byte)(PosY + i));
            }
            return List;
        }
        public List<byte> Xcoords(bool overflow)
        {
            var List = new List<byte>();
            for (byte i = 0; i < TileData.ObjectData.X; i++)
            {
                if (overflow && i+ PosX > 31)
                    List.Add((byte)(PosX + i));
                else if (!overflow && i + PosX < 32)
                    List.Add((byte)(PosX + i));
            }
            return List;
        }
        public List<byte> Zcoords(bool overflow)
        {
            var List = new List<byte>();
            for (byte i = 0; i < TileData.ObjectData.Z; i++)
            {
                if (overflow && i + PosZ > 31)
                    List.Add((byte)(PosZ + i));
                else if (!overflow && i + PosZ < 32)
                    List.Add((byte)(PosZ + i));
            }
            return List;
        }
        public ItemInstance(byte[] Record)
		{
			Bytes = Record;
            Id = Record[8];
            var tmp = Record[9] & 0x1F;
			Id += (short)(tmp * 256);
			PosX = (byte)(Record[9] >> 5);
			tmp = Record[10] & 0x3;
			PosX += (byte)(tmp << 3);
			PosY = (byte)(Record[10] >> 2);
			tmp = Record[11] & 0x1;
			PosY += (byte)(tmp << 6);
			PosZ = (byte)(Record[11] & 0x3E);
			PosZ >>= 1;
			Rotation = (byte)(Record[11] >> 6);
        }
	}
}
