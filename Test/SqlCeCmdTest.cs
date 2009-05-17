﻿
#if UNIT_TESTS
namespace SqlCeCmd.Tests
{
    using System;
    using System.IO;
    using NUnit.Framework;
    using CommandLine;
    using CommandLine.Text;

    [TestFixture]
    public sealed partial class SqlCeCmdFixture
    {
        private static ICommandLineParser parser = new CommandLineParser();

        [Test]
        public void ExerciseEngine()
        {
            string file = @"C:\test.sdf";
            if (System.IO.File.Exists(file))
            {
                System.IO.File.Delete(file);
            }
            SqlCeEngineHelper testEngine = new SqlCeEngineHelper(string.Format("Data Source={0};", file));
            testEngine.Execute(SqlCeEngineHelper.EngineAction.Create);
            Assert.IsTrue(System.IO.File.Exists(file), "Create OK");

            testEngine.Execute(SqlCeEngineHelper.EngineAction.Shrink);

            testEngine.Execute(SqlCeEngineHelper.EngineAction.Compact);

            testEngine.Execute(SqlCeEngineHelper.EngineAction.SetOption, "Data Source=;Password=123!!");

            //Use new password
            testEngine = new SqlCeEngineHelper(string.Format("Data Source={0};Password=123!!", file));

            testEngine.Execute(SqlCeEngineHelper.EngineAction.Shrink);

            //testEngine = new SqlCeEngineHelper("data source=C:\\Northwind.sdf");
            //testEngine.Execute(SqlCeEngineHelper.EngineAction.Upgrade);
        }

        [Test]
        public void ExerciseCommand()
        {
            SqlCeCommandHelper cmdHelper = new SqlCeCommandHelper(@"Data Source=C:\Data\SQLCE\ExportSqlCETest\Northwind.sdf;");
            Program.Options options = new Program.Options();
            options.OutputFile = @"C:\out.txt";
            options.Headers = 4;
            options.MakeXML = true;
            options.QueryText = "Insert Into [Tree] ([Id],[RowId],[TagId],[ParentTreeId],[Latitude],[Longitude],[Active],[Weight]) Values (1538,22,N'13-1225',null,null,null,1,null);";
            cmdHelper.RunCommand(options);
            //options.QueryText = "SELECT * FROM [Orders]";
            //cmdHelper.RunCommand(options);
            //options.QueryText = "SELECT * FROM [Order Details]";
            //cmdHelper.RunCommand(options);
            //options.QueryText = "SELECT * FROM [Employees]";
            //cmdHelper.RunCommand(options);            
        }

        [Test]
        public void ExerciseFileInput()
        {
            string file = @"C:\test.sdf";
            if (System.IO.File.Exists(file))
            {
                System.IO.File.Delete(file);
            }
            SqlCeEngineHelper testEngine = new SqlCeEngineHelper(string.Format("Data Source={0};", file));
            testEngine.Execute(SqlCeEngineHelper.EngineAction.Create);
            Assert.IsTrue(System.IO.File.Exists(file), "Create OK");

            SqlCeCommandHelper cmdHelper = new SqlCeCommandHelper(string.Format("Data Source={0};", file));
            Program.Options options = new Program.Options();
            options.QueryFile = @"C:\Data\SQLCE\ExportSqlCETest\nwind.sql";
            cmdHelper.RunCommands(options, true);
        }

        [Test]
        public void ExerciseParser()
        {
            string file = @"C:\test.sdf";
            if (System.IO.File.Exists(file))
            {
                System.IO.File.Delete(file);
            }
            SqlCeEngineHelper testEngine = new SqlCeEngineHelper(string.Format("Data Source={0};", file));
            testEngine.Execute(SqlCeEngineHelper.EngineAction.Create);
            Assert.IsTrue(System.IO.File.Exists(file), "Create OK");

            SqlCeCommandHelper cmdHelper = new SqlCeCommandHelper(string.Format("Data Source={0};", file));
            Program.Options options = new Program.Options();
            
            options.UseBatch = false;
            options.QueryFile = @"C:\Data\SQLCE\SqlCeCmdTest\northwind.sql";
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            cmdHelper.RunCommands(options, true);
            System.Diagnostics.Debug.Write("Time to run: " + sw.ElapsedMilliseconds.ToString());
            sw.Stop();
            cmdHelper.Dispose();

            //if (System.IO.File.Exists(file))
            //{
            //    System.IO.File.Delete(file);
            //}
            //SqlCeEngineHelper testEngine2 = new SqlCeEngineHelper(string.Format("Data Source={0};", file));
            //testEngine.Execute(SqlCeEngineHelper.EngineAction.Create);
            //Assert.IsTrue(System.IO.File.Exists(file), "Create OK");

            //SqlCeCommandHelper cmdHelper2 = new SqlCeCommandHelper(string.Format("Data Source={0};", file));
            //Program.Options options2 = new Program.Options();

            //options.UseBatch = false;
            //options.QueryFile = @"C:\Data\SQLCE\SqlCeCmdTest\nworders.sql";
            //sw.Start();
            //cmdHelper2.RunCommands(options, true);
            //System.Diagnostics.Debug.Write("QA: " + sw.ElapsedMilliseconds.ToString());
            //sw.Stop();
            //cmdHelper2.Dispose();
        }

        //[Test]
        //[ExpectedException(typeof(ArgumentNullException))]
        //public void WillThrowExceptionIfArgumentsArrayIsNull()
        //{
        //    parser.ParseArguments(null, new MockOptions());
        //}

    }
}
#endif
