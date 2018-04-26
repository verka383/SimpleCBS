using System;

using System.IO;
using System.Diagnostics;
using NUnit.Framework;
using System.Linq;
using System.Collections.Generic;
using CBS;

namespace SimpleCBS.Tests
{
    [TestFixture]
    public class Tests
    {
        readonly string outputPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(Tests).Assembly.Location),@"..\..\TestCases\Output\");
        static readonly string inputPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(Tests).Assembly.Location),@"..\..\TestCases\");

        static string[] TestSource()
        {
            return Directory.GetFiles(inputPath, "*.txt").Select(x=> System.IO.Path.GetFileName(x)).ToArray();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            //Directory.Delete(outputPath);
        }
        
        [TestCaseSource("TestSource")]
        public void TestSolution(string file)
        {
            Debug.WriteLine("Solving map:" + file);
            var solver = new CBS.CBSSolver(System.IO.Path.Combine(inputPath,file), new AStarSearch());

            var result = solver.RunSearch();
            if (result == null || result.Any(x => x.path == null || x.path.Count == 0))
                Assert.Fail("path not found");

            var totalCost = result.Sum(x => x.path.Count)-2;// start doesnt count

            switch (System.IO.Path.GetFileNameWithoutExtension(file))
            {
                case "Map 1": Assert.That(totalCost == 19);break;
                case "Map 2": Assert.That(totalCost == 17); break;
                case "Map 3": Assert.That(totalCost == 19); break;
                case "Map 4":
                    Assert.That(totalCost == 7); break;
                case "Map 5": Assert.That(totalCost == 15); break;
                case "Map 6": Assert.That(totalCost == 4); break;
                case "Map 7": /*Assert.That(totalCost == );*/ break;
                case "Map 8": Assert.That(totalCost == 8); break;
                case "Map 9": Assert.That(totalCost == 7); break;
                case "Map 10": Assert.That(totalCost == 13); break;
                //case "Map 11": Assert.That(totalCost == ); break;
                case "Map 12": Assert.That(totalCost == 14); break;
                case "Map 13": Assert.That(totalCost == 12); break;
                case "Map 14": Assert.That(totalCost == 6); break;
                case "Map 15": Assert.That(totalCost == 10); break;
                default: Assert.Fail("unknown file");break;
            }

            solver.WriteHumanReadableOutput(System.IO.Path.Combine(outputPath, file), result);
        }
    }
}
