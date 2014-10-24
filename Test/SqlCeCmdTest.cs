
#if UNIT_TESTS
namespace SqlCeCmd.Tests
{
    using System;
    using System.IO;
    using NUnit.Framework;
    using CommandLine;
    using CommandLine.Text;
    using System.Data.SqlServerCe;

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
        //-d"Data Source=C:\data\sqlce\test\nw40.sdf" -o -s -W -h 0 -n -q"SELECT * FROM Shippers"

        [Test]
        public void ExerciseCommand2()
        {
            SqlCeCommandHelper cmdHelper = new SqlCeCommandHelper(@"Data Source=C:\data\sqlce\test\northwind.sdf;");
            Program.Options options = new Program.Options();
            options.OutputFile = @"C:\data\sqlce\out.txt";
            options.Headers = 0;
            options.HideOutput = true;
            options.RemoveSpaces = true;
            //options.QueryText = "SELECT * FROM Shippers;";
            //cmdHelper.RunCommand(options);
            options.QueryText = "SELECT * FROM [Orders]";
            cmdHelper.RunCommand(options);
            //options.QueryText = "SELECT * FROM [Order Details]";
            //cmdHelper.RunCommand(options);
            //options.QueryText = "SELECT * FROM [Employees]";
            //cmdHelper.RunCommand(options);            
        }


        [Test]
        public void ExerciseCommandWithLoadImage()
        {
            SqlCeCommandHelper cmdHelper = new SqlCeCommandHelper(@"Data Source=C:\Data\SQLCE\Test\Northwind.sdf;");
            Program.Options options = new Program.Options();
            options.OutputFile = @"C:\out.txt";
            options.Headers = 4;
            options.MakeXML = true;
            options.QueryText = "INSERT INTO [Categories] ([Category ID],[Category Name],[Description],[Picture]) VALUES (1,N'Beverages',N'Soft drinks, coffees, teas, beer, and ale',SqlCeCmd_LoadImage(1fc193b9270e4bda856b9c9a2aecf943.blob));";
            cmdHelper.RunCommand(options);
        }

        
        //

        [Test]
        public void ExerciseFileInput()
        {
            string file = @"C:\data\test.sdf";
            if (System.IO.File.Exists(file))
            {
                System.IO.File.Delete(file);
            }
            SqlCeEngineHelper testEngine = new SqlCeEngineHelper(string.Format("Data Source={0};", file));
            testEngine.Execute(SqlCeEngineHelper.EngineAction.Create);
            Assert.IsTrue(System.IO.File.Exists(file), "Create OK");

            SqlCeCommandHelper cmdHelper = new SqlCeCommandHelper(string.Format("Data Source={0};", file));
            Program.Options options = new Program.Options();
            options.QueryFile = @"C:\Data\SQLCE\Test\ExportSqlCETest\northwind.sql";
            
            cmdHelper.RunCommands(options);

            SqlCeCommand cmd = new SqlCeCommand("SELECT COUNT(*) FROM [Order Details]", new SqlCeConnection(string.Format("Data Source={0};", file)));
            cmd.Connection.Open();
            int count = (int)cmd.ExecuteScalar();
            Assert.AreEqual(count, 2820);
        }

		[Test]
		public void CodePlex13272()
		{
			string file = @"C:\data\test.sdf";
			if (System.IO.File.Exists(file))
			{
				System.IO.File.Delete(file);
			}
			SqlCeEngineHelper testEngine = new SqlCeEngineHelper(string.Format("Data Source={0};", file));
			testEngine.Execute(SqlCeEngineHelper.EngineAction.Create);
			Assert.IsTrue(System.IO.File.Exists(file), "Create OK");

			SqlCeCommandHelper cmdHelper = new SqlCeCommandHelper(string.Format("Data Source={0};", file));
			Program.Options options = new Program.Options();
			options.QueryFile = @"C:\Data\SQLCE\sqlcecmdSVN\Test\goscript.sql";

			cmdHelper.RunCommands(options);

			SqlCeCommand cmd = new SqlCeCommand("SELECT COUNT(*) FROM [example]", new SqlCeConnection(string.Format("Data Source={0};", file)));
			cmd.Connection.Open();
			int count = (int)cmd.ExecuteScalar();
			Assert.AreEqual(count, 1);
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
            
            options.QueryFile = @"C:\Data\SQLCE\SqlCeCmdTest\northwind.sql";
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            cmdHelper.RunCommands(options);
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

    }
}
#endif
