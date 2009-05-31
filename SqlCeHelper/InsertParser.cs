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
            this.Rows = new List<List<KeyValuePair<int, object>>>();
        }

        public static string CheckTableName(string insertStatement)
        {
            string statement = insertStatement.ToLowerInvariant().Trim();
            if (!statement.StartsWith("insert "))
            {
                return null;
            }
            string tableName = statement.Substring(statement.IndexOf(" into ") + 6, statement.IndexOf(" (") - statement.IndexOf(" into ") - 6);
            if (tableName.StartsWith("["))
            {
                tableName = tableName.TrimEnd(new char[] { ']' }).TrimStart(new char[] { '[' });
            }
            return tableName;
        }

        public void AddRow(string insertStatement)
        {
            //try
            //{
                string statement = insertStatement.ToLowerInvariant().Trim();
                if (string.IsNullOrEmpty(this.TableName))
                {
                    this.TableName = CheckTableName(insertStatement);
                }
                
                string fieldList = statement.Substring(statement.IndexOf(" (") + 2, statement.IndexOf(") ") - statement.IndexOf(" (") - 2);
                string valueString = insertStatement.Substring(statement.IndexOf(" values ") + 8);
                valueString = valueString.Substring(valueString.IndexOf("(") + 1, valueString.LastIndexOf(")") - 1);

                string[] values = valueString.Split(new char[] { ',' });
                string[] fields = fieldList.Split(new char[] { ',' });

                // Combine string values with commas in them
                List<string> valueList = new List<string>(values.Length);
                valueList.AddRange(values);
                values = null;
                string oldVal = string.Empty;
                for ( int i = valueList.Count - 1; i > -1 ; i--)
                {
                    if (oldVal != string.Empty && !valueList[i].StartsWith("'") && !valueList[i].ToLowerInvariant().StartsWith("n'"))
                    {
                        oldVal = valueList[i] + "," + oldVal;
                        valueList.RemoveAt(i);
                    }
                    if (valueList[i].EndsWith("'"))
                    {
                        if (!valueList[i].StartsWith("'") && !valueList[i].ToLowerInvariant().StartsWith("n'"))
                        {
                            oldVal = valueList[i];
                            valueList.RemoveAt(i);
                        }
                    }
                    else if (valueList[i].StartsWith("'") || valueList[i].ToLowerInvariant().StartsWith("n'"))
                    {
                        if (!valueList[i].EndsWith("'"))
                        {
                            valueList[i] = valueList[i] + "," + oldVal;
                            oldVal = string.Empty;
                        }
                    }
                    
                    //Remove(i);
                }
                if (this.columnList.Count == 0)
                {
                    string sql = "SELECT     Column_name, is_nullable, data_type, character_maximum_length, numeric_precision, autoinc_increment, autoinc_seed, column_hasdefault, column_default, column_flags, numeric_scale, table_name, ordinal_position  " +
                    " FROM         information_schema.columns " +
                    " WHERE      (SUBSTRING(COLUMN_NAME, 1,5) <> '__sys')  " +
                    " AND TABLE_NAME = '" + this.TableName + "'" +
                    " ORDER BY ordinal_position ASC ";
                    using (System.Data.SqlServerCe.SqlCeCommand cmd = new System.Data.SqlServerCe.SqlCeCommand(sql))
                    {
                        cmd.Connection = this.conn;

                        using (SqlCeDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                columnList.Add(dr.GetString(0).ToLowerInvariant(), new Column
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
                                    TableName = dr.GetString(11),
                                    Ordinal = (dr.IsDBNull(12) ? 0 : dr.GetInt32(12))
                                    
                                });
                            }
                        }
                    }
                }

                List<KeyValuePair<int, object>> row = new List<KeyValuePair<int, object>>();

                
                if (fields.Length != valueList.Count)
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

                for (int i = 0; i < valueList.Count; i++)
                {
                    object val = null;
                    if (valueList[i].Trim() != "null")
                    {
                        if (valueList[i].ToLowerInvariant().StartsWith("n'"))
                        {
                            valueList[i] = valueList[i].Substring(2).TrimEnd(new char[] { '\'' });
                        }

                        if (valueList[i].StartsWith("'"))
                        {
                            valueList[i] = valueList[i].Substring(1).TrimEnd(new char[] { '\'' });
                        }

                        System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.InvariantCulture;
                        switch (columnList[fields[i]].DataType)
                        {
                            case "bigint":
                                val = Int64.Parse(valueList[i], ci);
                                break;

                            case "binary":
                                val = HexToData(valueList[i]);
                                break;

                            case "bit":
                                if (valueList[i] == "1")
                                {
                                    val = true;
                                }
                                else
                                {
                                    val = false;
                                }
                                break;

                            case "datetime":
                                val = ParseTs(valueList[i]);
                                break;

                            case "float":
                                val = Single.Parse(valueList[i], ci);
                                break;

                            case "image":
                                val = HexToData(valueList[i]);
                                break;

                            case "int":
                                val = Int32.Parse(valueList[i], ci);
                                break;

                            case "money":
                                val = Decimal.Parse(valueList[i], ci);
                                break;

                            case "nchar":
                                val = valueList[i].ToString(ci);
                                break;

                            case "ntext":
                                val = valueList[i].ToString(ci);
                                break;

                            case "numeric":
                                val = Decimal.Parse(valueList[i], ci);
                                break;

                            case "nvarchar":
                                val = valueList[i].ToString(ci);
                                break;

                            case "real":
                                val = Double.Parse(valueList[i], ci);
                                break;

                            case "rowversion":
                                val = HexToData(valueList[i]);
                                break;

                            case "smallint":
                                val = Int16.Parse(valueList[i], ci);
                                break;

                            case "tinyint":
                                val = (byte)(Int16.Parse(valueList[i], ci));
                                break;

                            case "uniqueidentifier":
                                val = new Guid(valueList[i]);
                                break;

                            case "varbinary":
                                val = HexToData(valueList[i]);
                                break;

                            default:
                                break;
                        }

                    }

                    row.Add(new KeyValuePair<int, object>(columnList[fields[i]].Ordinal - 1, val));
                }
                this.Rows.Add(row);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Adding row failed: " + insertStatement + "\n\r" + ex.ToString());
            //}
       }

        private static byte[] HexToData(string hexString)
        {    
            if (hexString == null)        
                return null;
            if (hexString.StartsWith("0x"))
            {
                if (hexString.Length > 2)
                {
                    hexString = hexString.Substring(2);
                }
                if (hexString.Length == 2)
                {
                    return new byte[0];
                }                
            }
            if (hexString.Length % 2 == 1)        
                hexString = '0' + hexString; 
            // Up to you whether to pad the first or last byte    
            byte[] data = new byte[hexString.Length / 2];    
            for (int i = 0; i < data.Length; i++)        
                data[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);    
            return data;
        }

        private static DateTime ParseTs(string dateTs)
        {
            //{ts '2001-07-08 00:00:00'}
                 //0123456789012345678
            dateTs = dateTs.Replace("{ts '", "").Replace("'}", "");
            return new DateTime(Int32.Parse(dateTs.Substring(0, 4)), Int32.Parse(dateTs.Substring(5, 2)), Int32.Parse(dateTs.Substring(8, 2)), Int32.Parse(dateTs.Substring(11, 2)), Int32.Parse(dateTs.Substring(14, 2)), Int32.Parse(dateTs.Substring(17, 2)));
        }

        public string TableName { get; set; }
        public List<List<KeyValuePair<int, object>>> Rows { get; set; }
        public Dictionary<string, Column> columnList = new Dictionary<string, Column>();
    }
}
