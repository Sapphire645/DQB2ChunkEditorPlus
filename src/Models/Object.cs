using System;

namespace DQB2ChunkEditor.Models
{
	public class ObjectItem
	{
        public short Id { get; set; } = -1;
		public uint Offset { get; set; }
        public short ChunkGrid { get; set; }
		public short Chunk { get; set; }
        public short PosX { get; set; }
        public short PosY { get; set; }
        public short PosZ { get; set; }
		public short Rotation { get; set; }
        public short ChunkRow => (short)(ChunkGrid / 64);
        public short ChunkCol => (short)(ChunkGrid % 64);
        public ObjectItem(byte[] Record)
		{
            Id = Record[8];
            var tmp = Record[9] & 0x1F;
			Id += (short)(tmp * 256);
			PosX = (short)(Record[9] >> 5);
			tmp = Record[10] & 0x3;
			PosX += (short)(tmp << 3);
			PosY = (short)(Record[10] >> 2);
			tmp = Record[11] & 0x1;
			PosY += (short)(tmp << 6);
			PosZ = (short)(Record[11] & 0x3E);
			PosZ >>= 1;
			Rotation = (short)(Record[11] >> 6);
        }
	}
}
