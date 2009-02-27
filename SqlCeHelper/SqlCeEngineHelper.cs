using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlServerCe;

namespace SqlCeCmd
{

    internal class SqlCeEngineHelper
    {
        private string connectionString;

        internal SqlCeEngineHelper(string connectionString)
        {
            this.connectionString = connectionString;
        }

        internal enum EngineAction
        {
            Undefined,
            Shrink,
            Compact,
            Create,
            Upgrade,
            RepairDelete,
            RepairRecover,
            GetInfo,
            SetOption
        }

        internal void Execute(EngineAction action)
        {
            Execute(action, null);
        }

        internal void Execute(EngineAction action, string newConnectionString)
        {
            // Specify connection string for new database options; The following 
            // tokens are valid:
            //      - Password
            //      - LCID
            //      - Encryption Mode
            //      - Case Sensitive
            // 
            // All other SqlCeConnection.ConnectionString tokens are ignored
            //
            //  engine.Compact("Data Source=; Password =a@3!7f$dQ;");

            if (action != EngineAction.GetInfo)
            {
                using (SqlCeEngine engine = new SqlCeEngine(connectionString))
                {
                    switch (action)
                    {
                        case EngineAction.Undefined:
                            break;
                        case EngineAction.Shrink:
                            engine.Shrink();
                            Console.WriteLine("Database successfully shrunk");
                            break;
                        case EngineAction.Compact:
                            engine.Compact(null);
                            Console.WriteLine("Database successfully compacted");
                            break;
                        case EngineAction.Upgrade:
                            engine.Upgrade();
                            Console.WriteLine("Database successfully upgraded");
                            break;
                        case EngineAction.Create:
                            engine.CreateDatabase();
                            Console.WriteLine("Database successfully created");
                            break;
                        case EngineAction.RepairDelete:
                            engine.Repair(null, RepairOption.DeleteCorruptedRows);
                            Console.WriteLine("Database successfully repaired");
                            break;
                        case EngineAction.RepairRecover:
                            engine.Repair(null, RepairOption.RecoverCorruptedRows);
                            Console.WriteLine("Database successfully repaired");
                            break;
                        case EngineAction.SetOption:
                            engine.Compact(newConnectionString);
                            Console.WriteLine("Database option(s) successfully changed");
                            break;
                        default:
                            break;
                    }
                }
            }
            else if (action == EngineAction.GetInfo)
            {
                using (SqlCeConnection cn = new SqlCeConnection(connectionString))
                {
                    cn.Open();
                    List<KeyValuePair<string, string>> valueList = new List<KeyValuePair<string, string>>();
                    // 3.5 or later only API
                    valueList = cn.GetDatabaseInfo();
                    valueList.Add(new KeyValuePair<string, string>("Database", cn.Database));
                    valueList.Add(new KeyValuePair<string, string>("ServerVersion", cn.ServerVersion));
                    foreach (KeyValuePair<string, string> pair in valueList)
                    {
                        Console.WriteLine(string.Format("{0}: {1}", pair.Key, pair.Value));
                    }
                }
            }
            return;
        }

    }
}
