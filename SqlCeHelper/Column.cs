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
        public YesNoOption IsNullable { get; set; }
        public string DataType { get; set; }
        public int CharacterMaxLength { get; set; }
        public int NumericPrecision { get; set; }
        public int NumericScale { get; set; }
        public Int64 AutoIncrementBy { get; set; }
        public Int64 AutoIncrementSeed { get; set; }
        public bool ColumnHasDefault { get; set; }
        public string ColumnDefault { get; set; }
        public bool RowGuidCol { get; set; }
        public string TableName { get; set; }
        public int Ordinal { get; set; }
    }
}
