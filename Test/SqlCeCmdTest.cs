
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
