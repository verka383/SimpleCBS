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
        readonly string outputPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(Tests).Assembly.Location), @"..\..\TestCases\Output\");
        static readonly string inputPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(Tests).Assembly.Location), @"..\..\TestCases\");

        static IEnumerable<TestConfiguration> TestSource()
        {
            var result = Directory.GetFiles(inputPath, "*.txt").Select(x => new TestConfiguration { file = System.IO.Path.GetFileName(x), k = 0 })
                .Concat(Directory.GetFiles(inputPath, "*.txt").Select(x => new TestConfiguration { file = System.IO.Path.GetFileName(x), k = 1 }))
                .ToArray();
            int[] sizes = new int[] { 19, 17, 19, 7, 15, 4, 46, 8, 7, 13, 14, 12, 6, 10, // k=0
                                      20, 18, 20, 7, 15, 4, 47, 8, 7, 13, 15, 12, 6, 11 }; // k=1
            for (int i = 0; i < result.Length; i++) result[i].solutionSize = sizes[i];
            return result;
        }

        public struct  TestConfiguration {
            public string file;
            public int k;
            public int solutionSize;
            public override string ToString()
            {
                return string.Format("file={0}, k={1}, solution={2}",file,k,solutionSize);
            }
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            //Directory.Delete(outputPath);
        }
        
        [TestCaseSource("TestSource")]
        public void TestSolution(TestConfiguration config)
        {
            Debug.WriteLine("Solving map:" + config.file);
            var solver = new CBS.CBSSolver(System.IO.Path.Combine(inputPath,config.file), new AStarSearch());

            var result = solver.RunSearch(config.k);
            if (result == null || result.Any(x => x.path == null || x.path.Count == 0))
                Assert.Fail("path not found");

            var totalCost = result.Sum(x => x.path.Count)-2;// start doesnt count
            Assert.That(totalCost == config.solutionSize, "Expected solution of size {0} but got {1}", config.solutionSize, totalCost);           

            solver.WriteHumanReadableOutput(System.IO.Path.Combine(outputPath, config.file), result);
        }
    }
}
