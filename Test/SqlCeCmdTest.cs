
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

            testEngine.Execute(SqlCeEngineHelper.EngineAction.SetPassword, "123!!");

            //Use new password
            testEngine = new SqlCeEngineHelper(string.Format("Data Source={0};Password=123!!", file));

            testEngine.Execute(SqlCeEngineHelper.EngineAction.Shrink);
        }

        [Test]
        public void ExerciseCommand()
        {
            SqlCeCommandHelper cmdHelper = new SqlCeCommandHelper(@"Data Source=C:\Data\SQLCE\ExportSqlCETest\Northwind.sdf;");
            Program.Options options = new Program.Options();
            options.QueryText = "SELECT * FROM [Test]";
            cmdHelper.RunCommands(options);
            options.QueryText = "SELECT * FROM [Orders]";
            cmdHelper.RunCommands(options);
            options.QueryText = "SELECT * FROM [Order Details]";
            cmdHelper.RunCommands(options);
            options.QueryText = "SELECT * FROM [Employees]";
            cmdHelper.RunCommands(options);            
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


        //[Test]
        //[ExpectedException(typeof(ArgumentNullException))]
        //public void WillThrowExceptionIfArgumentsArrayIsNull()
        //{
        //    parser.ParseArguments(null, new MockOptions());
        //}

    }
}
#endif
