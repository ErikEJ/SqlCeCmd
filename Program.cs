using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace SqlCeCmd
{
    internal sealed class Program
    {

        private enum Action
        { 
            Undefined,
            Query,
            QueryFromFile,
            PasswordChange,
            RunEngineCommand
        }

        private static readonly HeadingInfo headingInfo = new HeadingInfo("SqlCeCmd", "0.1");

        internal class Options
        {
            #region Standard Option Attribute
            [Option("d", null,
                    Required = true,
                    HelpText = "SQL Compact connection string")]
            public string ConnectionString = String.Empty;

            //[ -e shrink | compact | create | repairdelete | repairrecover | getinfo ]
            //[ -z new password ]
            //[ -q "cmdline query" ]
            //[ -i input_file(s) ] 

            //The 4 command options
            [Option("e", null,
                    HelpText = "Run SQL Compact engine actions: \r\n        shrink|compact|create|repairdelete|repairrecover")]
            public SqlCeEngineHelper.EngineAction EngineAction = SqlCeEngineHelper.EngineAction.Undefined;

            [Option("z", null,
                    HelpText = "Change database password")]
            public string NewPassword = String.Empty;

            [Option("q", null,
                    HelpText = "Command line query")]
            public string QueryText = String.Empty;

            [Option("i", null,
                    HelpText = "SQL query input file")]
            public string QueryFile = String.Empty;

            //[ -R use client regional settings ]
            //[ -s col_separator ] [ -w column_width ] 
            //[ -W remove trailing spaces ]
            //[ -k [ 1 | 2 ] remove[replace] control characters ] 
            //[ -y display_width ] [-Y display_width ]

            //Output formatting options
            [Option("R", null,
                   HelpText = "Use client regional settings")]
            public bool UseCurrentCulture = false;

            [Option("s", null,
                     HelpText = "Column separator")]
            public Char ColumnSeparator = Convert.ToChar(" ");

            //Greater than 8 and less than 65536
            [Option("w", null,
                    HelpText = "Column width")]
            public int ColumnWidth = 80;

            [Option("W", null,
                    HelpText = "Remove trailing spaces")]
            public bool RemoveSpaces = false;

            [Option("k", null,
                    HelpText = "Remove or replace control characters. Range 0,1,2")]
            public int RemReplControlChars = 0;

            [Option("y", null,
                    HelpText = "Display width (image, ntext)")]
            public int ColumnWidthBlob = 0; // 1048576 = 1 MB

            [Option("Y", null,
                    HelpText = "Display width (nchar, nvarchar)")]
            public int ColumnWidthChar = 256;

            #endregion

            #region Specialized Option Attribute

            [HelpOption("?", null,  
                    HelpText = "Display this help screen.")]
            public string GetUsage()
            {
                HelpText help = new HelpText(Program.headingInfo);
                help.Copyright = new CopyrightInfo("Erik Ejlskov Jensen", 2009, 2009);
                help.AddPreOptionsLine("Contact me at my blog: http://erikej.blogspot.com");
                //help.AddPreOptionsLine("Usage: SampleApp -rMyData.in -wMyData.out --calculate");
                //help.AddPreOptionsLine(string.Format("       SampleApp -rMyData.in -i -j{0} file0.def file1.def", 9.7));
                //help.AddPreOptionsLine("       SampleApp -rMath.xml -wReport.bin -o *;/;+;-");
                help.AddOptions(this);
                return help;
            }
            #endregion
        }

        private static void Main(string[] args)
        {
            try
            {
                Options options = new Options();
                ICommandLineParser parser = new CommandLineParser();
                if (parser.ParseArguments(args, options, Console.Error))
                {
                    //Console.WriteLine("New password: {0}", options.NewPassword);
                    //Console.WriteLine();

                    //Console.WriteLine("Command line query: {0}", options.QueryText);
                    //Console.WriteLine();

                    int actionCount = 0;
                    Action action = Action.Undefined;
                    if (options.EngineAction != SqlCeEngineHelper.EngineAction.Undefined) { actionCount++; action = Action.RunEngineCommand; }
                    if (!string.IsNullOrEmpty(options.NewPassword)) { actionCount++; action = Action.PasswordChange; }
                    if (!string.IsNullOrEmpty(options.QueryText)) { actionCount++; action = Action.Query; }
                    if (!string.IsNullOrEmpty(options.QueryFile)) { actionCount++; action = Action.QueryFromFile; }

                    if (actionCount == 0)
                    {
                        Console.WriteLine("Either -q, -i, -e or -z required");
                        Console.ReadLine();
                        Environment.Exit(1);                        
                    }
                    // actionCount must be exactly 1
                    if (actionCount > 1)
                    {
                        Console.WriteLine("Only one of either -q, -i, -e or -z required");
                        Console.ReadLine();
                        Environment.Exit(1);                                                
                    }
                    switch (action)
                    {
                        case Action.Undefined:
                            break;
                        case Action.Query:
                            using (SqlCeCommandHelper cmdHelper = new SqlCeCommandHelper(options.ConnectionString))
                            {
                                cmdHelper.RunCommands(options);
                            }
                            break;
                        case Action.QueryFromFile:
                            using (SqlCeCommandHelper cmdHelper = new SqlCeCommandHelper(options.ConnectionString))
                            {
                                cmdHelper.RunCommands(options, true);
                            }
                            break;
                        case Action.PasswordChange:
                            if (!string.IsNullOrEmpty(options.NewPassword))
                            {
                                SqlCeEngineHelper pwEngine = new SqlCeEngineHelper(options.ConnectionString);
                                pwEngine.Execute(options.EngineAction, options.NewPassword);
                            }
                            else
                            {
                                Console.WriteLine("New password required");
                            }
                            break;
                        case Action.RunEngineCommand:
                            SqlCeEngineHelper engine = new SqlCeEngineHelper(options.ConnectionString);
                            engine.Execute(options.EngineAction);
                            break;
                        default:
                            break;
                    }
                    Console.ReadLine();
                    

                    //Console.WriteLine();
                    //if (options.OutputFile.Length > 0)
                    //{
                    //    headingInfo.WriteMessage(string.Format("Writing elaborated data: {0} ...", options.OutputFile));
                    //}
                    //else
                    //{
                    //    headingInfo.WriteMessage("Elaborated data:");
                    //    Console.WriteLine("[...]");
                    //}
                    
                    Environment.Exit(0);
                }
                else
                {
                    Console.ReadLine();
                    Environment.Exit(1);
                }
            }

            catch (System.Data.SqlServerCe.SqlCeException e)
            {
                SqlCeUtility.ShowErrors(e);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.ToString());
            }

        }
    }
}
