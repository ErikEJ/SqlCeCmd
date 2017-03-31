using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;
using CommandLine.Text;
using System.IO;
using System.Globalization;

namespace SqlCeCmd
{
    internal sealed class Program
    {

        private enum Action
        { 
            Undefined,
            Query,
            QueryFromFile,
            OptionChange,
            RunEngineCommand
        }

        private static readonly HeadingInfo headingInfo = new HeadingInfo(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());

        internal class Options
        {
            #region Standard Option Attribute
            [Option("d", null,
                    Required = true,
                    HelpText = "SQL Compact ADO.NET connection string\r\n")]
            public string ConnectionString = String.Empty;

            //The 4 command options
            [Option("e", null,
                    HelpText = "Run SQL Compact engine actions: \r\n        shrink|compact|create|upgrade|verify|repairdelete|repairrecover")]
            public SqlCeEngineHelper.EngineAction EngineAction = SqlCeEngineHelper.EngineAction.Undefined;

            [Option("z", null,
                    HelpText = "Change database options:  \r\n        Password,Encryption Mode,Locale Id,Case Sensitivity")]
            public string NewOptions = String.Empty;

            [Option("q", null,
                    HelpText = "Command line query")]
            public string QueryText = String.Empty;

            [Option("i", null,
                    HelpText = "SQL query input file\r\n")]
            public string QueryFile = String.Empty;

            //Output options
            [Option("n", null,
                    HelpText = "Hide query output\r\n")]
            public bool HideOutput = false;

            [Option("o", null,
                HelpText = "Output file")]
            public string OutputFile = String.Empty;

            [Option("v", null,
                HelpText = "Display database information")]
            public bool ShowInfo = false;

            //Output formatting options
            [Option("R", null,
                   HelpText = "Use client regional settings")]
            public bool UseCurrentCulture = false;

            [Option("h", null,
                   HelpText = "Headers - 0 to Int32.MaxValue")]
            public int Headers = Int32.MinValue;

            [Option("s", null,
                     HelpText = "Column separator")]
            public Char ColumnSeparator = Convert.ToChar(" ", CultureInfo.InvariantCulture);

            [Option("W", null,
                    HelpText = "Remove trailing spaces")]
            public bool RemoveSpaces = false;

            [Option("x", null,
                    HelpText = "Output SELECTs as XML")]
            public bool MakeXML = false;

            #endregion

            #region Specialized Option Attribute

            [HelpOption("?", null,  
                    HelpText = "Display this help screen")]
            public string GetUsage()
            {
                HelpText help = new HelpText(Program.headingInfo);
                help.Copyright = new CopyrightInfo("Erik Ejlskov Jensen", 2010);
                help.AddPreOptionsLine("Contact me at my blog: http://erikej.blogspot.com");
                help.AddPreOptionsLine("Check for updates at: http://sqlcecmd.codeplex.com");
                help.AddPreOptionsLine("");
                help.AddPreOptionsLine("Sample usage:");
                help.AddPreOptionsLine("sqlcecmd -d \"Data Source=C:\\nw.sdf\" -q \"SELECT * FROM Shippers\"");
                help.AddOptions(this);
                return help;
            }
            #endregion
        }

        private static void Main(string[] args)
        {
            FileStream ostrm = null;
            StreamWriter writer = null;
            try
            {
                Options options = new Options();
                ICommandLineParser parser = new CommandLineParser();
                if (parser.ParseArguments(args, options, Console.Error))
                {
                    int actionCount = 0;
                    Action action = Action.Undefined;
                    if (options.EngineAction != SqlCeEngineHelper.EngineAction.Undefined) { actionCount++; action = Action.RunEngineCommand; }
                    if (!string.IsNullOrEmpty(options.NewOptions)) { actionCount++; action = Action.OptionChange; }
                    if (!string.IsNullOrEmpty(options.QueryText)) { actionCount++; action = Action.Query; }
                    if (!string.IsNullOrEmpty(options.QueryFile)) { actionCount++; action = Action.QueryFromFile; }

                    if (actionCount == 0)
                    {
                        Console.Error.WriteLine("Either -q, -i, -e or -z required");
                        Environment.Exit(1);                        
                    }
                    // actionCount must be exactly 1
                    if (actionCount > 1)
                    {
                        Console.Error.WriteLine("Only one of either -q, -i, -e or -z required");
                        Environment.Exit(1);                                                
                    }
                    if (options.Headers != Int32.MinValue)
                    { 
                        if (options.Headers >= 0 && options.Headers <= Int32.MaxValue)
                        {}
                        else
                        {
                            Console.Error.WriteLine("Headers value must be a value between 0 and 2147483647");
                            Environment.Exit(1);                                                
                        }
                    }
                    if (!string.IsNullOrEmpty(options.OutputFile))
                    {
                        try
                        {
                            ostrm = new FileStream(options.OutputFile, FileMode.Create, FileAccess.Write);
                            writer = new StreamWriter(ostrm);
                        }
                        catch (Exception e)
                        {
                            Console.Error.WriteLine(string.Format(CultureInfo.InvariantCulture, "Cannot open {0} for writing", options.OutputFile));
                            Console.Error.WriteLine(e.Message);
                            Environment.Exit(1);
                            return;
                        }
                        Console.SetOut(writer);
                    }
                    if (options.ShowInfo)
                    {
                        SqlCeEngineHelper engine = new SqlCeEngineHelper(options.ConnectionString);
                        engine.Execute(SqlCeEngineHelper.EngineAction.GetInfo, null);
                    }
                    switch (action)
                    {
                        case Action.Undefined:
                            break;
                        case Action.Query:
                            using (SqlCeCommandHelper cmdHelper = new SqlCeCommandHelper(options.ConnectionString))
                            {
                                cmdHelper.RunCommand(options);
                            }
                            break;
                        case Action.QueryFromFile:
                            using (SqlCeCommandHelper cmdHelper = new SqlCeCommandHelper(options.ConnectionString))
                            {
                                cmdHelper.RunCommands(options);
                            }
                            break;
                        case Action.OptionChange:
                            if (!string.IsNullOrEmpty(options.NewOptions))
                            {
                                SqlCeEngineHelper pwEngine = new SqlCeEngineHelper(options.ConnectionString);
                                options.EngineAction = SqlCeEngineHelper.EngineAction.SetOption;
                                pwEngine.Execute(options.EngineAction, options.NewOptions);
                            }
                            else
                            {
                                Console.Error.WriteLine("Connection string with new options required");
                            }
                            break;
                        case Action.RunEngineCommand:
                            SqlCeEngineHelper engine = new SqlCeEngineHelper(options.ConnectionString);
                            engine.Execute(options.EngineAction);
                            break;
                        default:
                            break;
                    }

                    if (writer != null)
                        writer.Close();
                    if (ostrm != null)
                        ostrm.Close();
                                        
                    Environment.Exit(0);
                }
                else
                {
                    Environment.Exit(1);
                }
            }

            catch (System.Data.SqlServerCe.SqlCeException e)
            {
                SqlCeUtility.ShowErrors(e);
                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error: " + ex.ToString());
                Environment.Exit(1);
            }

        }
    }
}
