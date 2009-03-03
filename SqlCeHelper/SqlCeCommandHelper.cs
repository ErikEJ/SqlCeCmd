using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlServerCe;

namespace SqlCeCmd
{
    internal class SqlCeCommandHelper : IDisposable 
    {
        private SqlCeConnection conn = new SqlCeConnection();

        private enum CommandExecute
        {
            Undefined,
            DataReader,
            NonQuery
        }
        
        internal SqlCeCommandHelper(string connectionString)
        {
            this.conn.ConnectionString = connectionString;
            this.conn.Open();
        }

        internal void RunCommands(SqlCeCmd.Program.Options options, bool useFile)
        {
            System.IO.StreamReader sr = System.IO.File.OpenText(options.QueryFile);
            StringBuilder sb = new StringBuilder(10000);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line.Equals("GO", StringComparison.InvariantCultureIgnoreCase))
                {
                    Console.WriteLine("Executing: " + Environment.NewLine + sb.ToString());
                    options.QueryText = sb.ToString();
                    RunCommands(options);
                    sb.Remove(0, sb.Length);
                }
                else
                {
                    if (!line.StartsWith("--"))
                    {
                        sb.Append(line);
                        sb.Append(Environment.NewLine);
                    }
                }
            }
        }

        internal void RunCommands(SqlCeCmd.Program.Options options)
        {
            //TODO Parse formatting options (and implement use of same)
            using (SqlCeCommand cmd = new SqlCeCommand(options.QueryText))
            {
                CommandExecute execute = FindExecuteType(options.QueryText);
                int rows = 0;
                if (execute != CommandExecute.Undefined)
                {
                    if (execute == CommandExecute.DataReader)
                    {
                        rows = RunDataReader(cmd, conn);
                    }
                    if (execute == CommandExecute.NonQuery)
                    {
                        rows = RunNonQuery(cmd, conn);
                    }
                    Console.WriteLine();
                    Console.WriteLine(string.Format("({0} rows affected)", rows.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                }
                else
                {
                    Console.WriteLine("Invalid command text");
                }
            }
        }

        private int RunDataReader(SqlCeCommand cmd, SqlCeConnection conn)
        {
            cmd.Connection = conn;
            SqlCeDataReader rdr = cmd.ExecuteReader();
            int rows = 0;
            int maxWidth = 256;
            List<Column> headings = new List<Column>();
            for (int i = 0; i < rdr.FieldCount; i++)
            {
                // 18 different types
                // Calculate width as max of name or data type based width
                switch (rdr.GetDataTypeName(i))
                {
                    case "BigInt":
                        int width = Math.Max(20, rdr.GetName(i).Length);
                        headings.Add(new Column { Name = rdr.GetName(i), Width = width, PadLeft = true });
                        break;

                    case "Binary":
                        width = Math.Max(GetFieldSize(conn, rdr.GetName(i), maxWidth, cmd.CommandText), rdr.GetName(i).Length) + 2;
                        headings.Add(new Column { Name = rdr.GetName(i), Width = width, PadLeft = false });
                        break;

                    case "Bit":
                        width = Math.Max(5, rdr.GetName(i).Length);
                        headings.Add(new Column { Name = rdr.GetName(i), Width = width, PadLeft = true });
                        break;

                    case "DateTime":
                        width = Math.Max(20, rdr.GetName(i).Length);
                        headings.Add(new Column { Name = rdr.GetName(i), Width = width, PadLeft = false });
                        break;

                    case "Float":
                        width = Math.Max(24, rdr.GetName(i).Length);
                        headings.Add(new Column { Name = rdr.GetName(i), Width = width, PadLeft = true });
                        break;

                    case "Image":
                        width = Math.Max(GetFieldSize(conn, rdr.GetName(i), maxWidth, cmd.CommandText), rdr.GetName(i).Length) + 2;
                        headings.Add(new Column { Name = rdr.GetName(i), Width = width, PadLeft = false });
                        break;

                    case "Int":
                        width = Math.Max(11, rdr.GetName(i).Length);
                        headings.Add(new Column{ Name = rdr.GetName(i), Width = width, PadLeft = true} );
                        break;

                    case "Money":
                        width = Math.Max(21, rdr.GetName(i).Length);
                        headings.Add(new Column { Name = rdr.GetName(i), Width = width, PadLeft = true });
                        break;

                    case "NChar":
                        width = Math.Max(GetFieldSize(conn, rdr.GetName(i), maxWidth, cmd.CommandText), rdr.GetName(i).Length);
                        headings.Add(new Column { Name = rdr.GetName(i), Width = width, PadLeft = false });
                        break;

                    case "NText":
                        width = Math.Max(GetFieldSize(conn, rdr.GetName(i), maxWidth, cmd.CommandText), rdr.GetName(i).Length);
                        headings.Add(new Column { Name = rdr.GetName(i), Width = width, PadLeft = false });
                        break;

                    case "Numeric":
                        width = Math.Max(21, rdr.GetName(i).Length);
                        headings.Add(new Column { Name = rdr.GetName(i), Width = width, PadLeft = true });
                        break;

                    case "NVarChar":
                        width = Math.Max(GetFieldSize(conn, rdr.GetName(i), maxWidth, cmd.CommandText), rdr.GetName(i).Length);
                        headings.Add(new Column { Name = rdr.GetName(i), Width = width, PadLeft = false });
                        break;

                    case "Real":
                        width = Math.Max(14, rdr.GetName(i).Length);
                        headings.Add(new Column { Name = rdr.GetName(i), Width = width, PadLeft = true });
                        break;

                    case "RowVersion":
                        width = Math.Max(8, rdr.GetName(i).Length) + 2;
                        headings.Add(new Column { Name = rdr.GetName(i), Width = width, PadLeft = false });
                        break;

                    case "SmallInt":
                        width = Math.Max(6, rdr.GetName(i).Length);
                        headings.Add(new Column { Name = rdr.GetName(i), Width = width, PadLeft = true });
                        break;

                    case "TinyInt":
                        width = Math.Max(3, rdr.GetName(i).Length);
                        headings.Add(new Column { Name = rdr.GetName(i), Width = width, PadLeft = true });
                        break;

                    case "UniqueIdentifier":
                        width = Math.Max(36, rdr.GetName(i).Length);
                        headings.Add(new Column { Name = rdr.GetName(i), Width = width, PadLeft = false });
                        break;

                    case "VarBinary":
                        width = Math.Max(GetFieldSize(conn, rdr.GetName(i), maxWidth, cmd.CommandText), rdr.GetName(i).Length) + 2;
                        headings.Add(new Column { Name = rdr.GetName(i), Width = width, PadLeft = false });
                        break;

                    default:
                        break;
                }                
            }
            while (rdr.Read())
            {
                if (rows == 0)
                {
                    // Write headers
                    for (int x = 0; x < rdr.FieldCount; x++)
                    {
                        Console.Write(headings[x].Name.PadRight(headings[x].Width));
                        Console.Write(" ");
                    }
                    Console.WriteLine();
                    for (int x = 0; x < rdr.FieldCount; x++)
                    {
                        System.Text.StringBuilder sb = new System.Text.StringBuilder();
                        sb.Append('-', headings[x].Width);
                        Console.Write(sb.ToString());
                        Console.Write(" ");
                    }
                    Console.WriteLine();
                }
                for (int i = 0; i < rdr.FieldCount; i++)
                {                   
                    //TODO Format output better...
                    if (!rdr.IsDBNull(i))
                    {
                        string value = string.Empty;
                        string fieldType = rdr.GetDataTypeName(i);
                        if (fieldType == "Image" || fieldType == "VarBinary" || fieldType == "Binary" || fieldType == "RowVersion")
                        {
                            Byte[] buffer = (Byte[])rdr[i];
                            StringBuilder sb = new StringBuilder();
                            sb.Append("0x");
                            for (int y = 0; y < (headings[i].Width -2) / 2; y++)
                            {
                                sb.Append(buffer[y].ToString("X2"));
                            }
                            value = sb.ToString();
                        }
                        else
                        {
                            value = Convert.ToString(rdr[i], System.Globalization.CultureInfo.InvariantCulture);
                        }

                        if (headings[i].PadLeft)
                        {
                            Console.Write(value.PadLeft(headings[i].Width));
                        }
                        else 
                        {
                            Console.Write(value.PadRight(headings[i].Width));
                        }
                    }
                    else
                    {
                        Console.Write("NULL".PadRight(headings[i].Width));
                    }
                    Console.Write(" ");
                }                
                rows++;
                Console.WriteLine();
            }            
            return rows;
        }

        private int RunNonQuery(SqlCeCommand cmd, SqlCeConnection conn)
        {
            cmd.Connection = conn;
            return cmd.ExecuteNonQuery();
        }

        private CommandExecute FindExecuteType(string commandText)
        {
            if (string.IsNullOrEmpty(commandText))
            {
                return CommandExecute.Undefined;
            }
            
            string test = commandText.Trim();

            if (test.ToLowerInvariant().StartsWith("select "))
            {
                return CommandExecute.DataReader;
            }
            else
            {
                return CommandExecute.NonQuery;
            }
        }

        private int GetFieldSize(SqlCeConnection conn, string fieldName, int maxWidth, string commandText)
        { 
            using (SqlCeCommand cmdSize = new SqlCeCommand(commandText))
            {
                cmdSize.Connection = conn;
                SqlCeDataReader rdr = cmdSize.ExecuteReader(System.Data.CommandBehavior.SchemaOnly | System.Data.CommandBehavior.KeyInfo);
                System.Data.DataTable schemaTable = rdr.GetSchemaTable();
                System.Data.DataView schemaView = new System.Data.DataView(schemaTable);
                schemaView.RowFilter = string.Format("ColumnName = '{0}'", fieldName);
                if (schemaView.Count > 0)
                {
                    string colName = schemaView[0].Row["BaseColumnName"].ToString();
                    string tabName = schemaView[0].Row["BaseTableName"].ToString();
                    using (SqlCeCommand cmd = new SqlCeCommand(string.Format("SELECT CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = '{0}' AND TABLE_NAME = '{1}'", colName, tabName)))
                    {
                        cmd.Connection = conn;
                        Object val = cmd.ExecuteScalar();
                        if (val != null)
                        {
                            if ((int)val < maxWidth)
                            {
                                return (int)val;
                            }
                            else
                            {
                                return maxWidth;
                            }
                        }
                    }
                }
            }
            return -1;            
        }

        #region IDisposable Members
        public void Dispose()
        {
            if (conn != null)
            {
                conn.Dispose();
            }
        }
        #endregion
    }
}
