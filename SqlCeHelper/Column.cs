using System;
using System.Collections.Generic;
using System.Text;

namespace SqlCeCmd
{
    public struct Column
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public bool PadLeft { get; set; }
        //public bool Truncate { get; set; }
        //public int MaxWidth { get; set; }
    }
}
