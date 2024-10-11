using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace DQB2ChunkEditor.Models
{
    public class SignText
    {
        public int Size { get; set; }
        public int Index { get; set; }
        public string Line { get { return mLine; } set { byte[] byteArray = Encoding.Default.GetBytes(value); if (byteArray.Length < Size+1) { mLine = value; } else
                {
                    var NameBytes = new byte[Size];
                    Array.Copy(byteArray, 0, NameBytes, 0, Size);
                    Line = System.Text.Encoding.Default.GetString(NameBytes);
                } } }
        private string mLine { get; set; }
    }
}
