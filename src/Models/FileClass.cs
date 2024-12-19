using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQB2ChunkEditor.Models
{
    internal static class FileClass
    {
        public static IEnumerable<string> ItemFile { get; } = System.IO.File.ReadLines("Data/Items.txt");
        public static IEnumerable<string> BlockFile { get; } = System.IO.File.ReadLines("Data/BlockExtra.txt");
    }
}
