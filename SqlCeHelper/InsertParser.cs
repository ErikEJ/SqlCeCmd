using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlServerCe;

namespace SqlCeCmd
{
    public enum YesNoOption
    {
        YES
        , NO
    }

    internal class InsertParser
    {
        private SqlCeConnection conn;

        public InsertParser(SqlCeConnection conn)
        {
            this.conn = conn;
        }

        public void AddRow(string insertStatement)
        {
            string statement = "Insert Into [Tree] ([Id],[RowId],[TagId],[ParentTreeId],[Latitude],[Longitude],[Active],[Weight]) Values (1538,22,N'13- 1225',null,null,null,1,null);";
            statement = statement.ToLowerInvariant().Trim();
            //Insert Into [Tree] ([Id],[RowId],[TagId],[ParentTreeId],[Latitude],[Longitude],[Active],[Weight]) Values (1539,22,N'13-1224',1538,0.000000000000000,0.000000000000000,1,null);

            if (string.IsNullOrEmpty(this.TableName))
            {
                this.TableName = statement.Substring(statement.IndexOf(" into ") + 6, statement.IndexOf(" (") - statement.IndexOf(" into ") - 6);
                if (this.TableName.StartsWith("["))
                {
                    this.TableName = this.TableName.TrimEnd(new char[] { ']' }).TrimStart(new char[] { '[' });
                }
            }

            string fieldList = statement.Substring(statement.IndexOf(" (") + 2, statement.IndexOf(") ") - statement.IndexOf(" (") - 2);

            string valueList = statement.Substring(statement.IndexOf(" values ") + 8);
            valueList = valueList.Substring(valueList.IndexOf("(") + 1, valueList.LastIndexOf(")") - 1);

            string[] values = valueList.Split(new char[] { ',' });
            string[] fields = fieldList.Split(new char[] { ',' });

            if (this.columnList.Count == 0)
            {
                string sql = "SELECT     Column_name, is_nullable, data_type, character_maximum_length, numeric_precision, autoinc_increment, autoinc_seed, column_hasdefault, column_default, column_flags, numeric_scale, table_name  " +
                " FROM         information_schema.columns " +
                " WHERE      (column_name NOT LIKE '__sys%') " +
                " AND TABLE_NAME = '" + this.TableName + "'" +
                " ORDER BY ordinal_position ASC ";
                using (System.Data.SqlServerCe.SqlCeCommand cmd = new System.Data.SqlServerCe.SqlCeCommand(sql))
                {
                    cmd.Connection = this.conn;

                    SqlCeDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                    columnList.Add(dr.GetString(0), new Column
                    {
                        Name = dr.GetString(0),
                        IsNullable = (YesNoOption)Enum.Parse(typeof(YesNoOption), dr.GetString(1)),
                        DataType = dr.GetString(2),
                        CharacterMaxLength = (dr.IsDBNull(3) ? 0 : dr.GetInt32(3)),
                        NumericPrecision = (dr.IsDBNull(4) ? 0 : Convert.ToInt32(dr[4], System.Globalization.CultureInfo.InvariantCulture)), 
                        AutoIncrementBy = (dr.IsDBNull(5) ? 0 : Convert.ToInt64(dr[5], System.Globalization.CultureInfo.InvariantCulture)),
                        AutoIncrementSeed = (dr.IsDBNull(6) ? 0 : Convert.ToInt64(dr[6], System.Globalization.CultureInfo.InvariantCulture)),
                        ColumnHasDefault = (dr.IsDBNull(7) ? false : dr.GetBoolean(7)),
                        ColumnDefault = (dr.IsDBNull(8) ? string.Empty : dr.GetString(8).Trim()),
                        RowGuidCol = (dr.IsDBNull(9) ? false : dr.GetInt32(9) == 378),
                        NumericScale = (dr.IsDBNull(10) ? 0 : Convert.ToInt32(dr[10], System.Globalization.CultureInfo.InvariantCulture)),
                        TableName = dr.GetString(11)
                    });
                    }

                }
            }

            List<KeyValuePair<string, object>> row = new List<KeyValuePair<string, object>>();

            if (fields.Length != values.Length)
            {
                throw new InvalidOperationException("Number of fields and values do not match: " + insertStatement);
            }

            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].StartsWith("["))
                {
                    fields[i] = fields[i].TrimEnd(new char[] { ']' }).TrimStart(new char[] { '[' });
                }            
            }

            for (int i = 0; i < values.Length; i++)
            {
                object val = null;
                if (columnList[fields[i]].AutoIncrementBy == 0 && values[i].Trim() != "null")
                    {
                    //  switch (ceDataType)
                    //{
                    //    case "bigint":
                    //        return System.Data.DbType.Int64;
                    //    case "binary":
                    //        return DbType.Binary;
                    //    case "bit":
                    //        return System.Data.DbType.Boolean;
                    //    case "datetime":
                    //        return System.Data.DbType.DateTime;
                    //    case "float":
                    //        return System.Data.DbType.Single;
                    //    case "image":
                    //        return System.Data.DbType.Binary;
                    //    case "int":
                    //        return System.Data.DbType.Int32;
                    //    case "money":
                    //        return System.Data.DbType.Currency;
                    //    case "nchar":
                    //        return System.Data.DbType.StringFixedLength;
                    //    case "ntext":
                    //        return DbType.String;
                    //    case "numeric":
                    //        return System.Data.DbType.VarNumeric;
                    //    case "nvarchar":
                    //        return System.Data.DbType.String;
                    //    case "real":
                    //        return System.Data.DbType.Double;
                    //    case "rowversion":
                    //        return System.Data.DbType.Binary;
                    //    case "smallint":
                    //        return System.Data.DbType.Int16;
                    //    case "tinyint":
                    //        return System.Data.DbType.Byte;
                    //    case "uniqueidentifier":
                    //        return DbType.Guid;
                    //    case "varbinary":
                    //        return DbType.Binary;

                    //TODO parse

                    switch (columnList[fields[i]].DataType)
                    {
                        case "bigint":
                            val = Int64.Parse(values[i]);
                            break;

                        case "binary":
                            break;

                        case "bit":
                            val = bool.Parse(values[i]);
                            break;

                        case "datetime":
                            val = DateTime.Parse(values[i], System.Globalization.CultureInfo.InvariantCulture);
                            break;

                        case "float":
                            
                            break;

                        case "image":
                            break;

                        case "int":
                            break;

                        case "money":
                            break;

                        case "nchar":
                            break;

                        case "ntext":
                            break;

                        case "numeric":
                            break;

                        case "nvarchar":
                            break;

                        case "real":
                            break;

                        case "rowversion":
                            break;

                        case "smallint":
                            break;

                        case "tinyint":
                            break;

                        case "uniqueidentifier":
                            break;

                        case "varbinary":
                            break;

                        default:
                            break;
                    }                

                    
                }

                row.Add(new KeyValuePair<string, object>(fields[i], val));
            }
            this.Rows.Add(row);            
       }

        public string TableName { get; set; }
        public List<List<KeyValuePair<string, object>>> Rows { get; set; }
        public Dictionary<string, Column> columnList = new Dictionary<string, Column>();
    }
}
