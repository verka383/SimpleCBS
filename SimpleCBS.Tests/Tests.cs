using System;

using System.IO;
using System.Diagnostics;
using NUnit.Framework;

namespace SimpleCBS.Tests
{
    [TestFixture]
    public class Tests
    {
        readonly string outputPath = Path.Combine(Path.GetDirectoryName(typeof(Tests).Assembly.Location),@"..\..\TestCases\Output");
        static readonly string inputPath = Path.Combine(Path.GetDirectoryName(typeof(Tests).Assembly.Location),@"..\..\TestCases\");

        static string[] TestSource()
        {
            return Directory.GetFiles(inputPath, "*.txt");
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            Directory.Delete(outputPath);
        }
        
        [TestCaseSource("TestSource")]
        public void TestSolution(string file)
        {
            Debug.WriteLine("Solving map:" + file);
            new CBS.CBSSolver(file, outputPath);
        }
    }
}
