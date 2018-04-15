﻿using System;

using System.IO;
using System.Diagnostics;
using NUnit.Framework;
using System.Linq;
using System.Collections.Generic;

namespace SimpleCBS.Tests
{
    [TestFixture]
    public class Tests
    {
        readonly string outputPath = Path.Combine(Path.GetDirectoryName(typeof(Tests).Assembly.Location),@"..\..\TestCases\Output\");
        static readonly string inputPath = Path.Combine(Path.GetDirectoryName(typeof(Tests).Assembly.Location),@"..\..\TestCases\");

        static string[] TestSource()
        {
            return Directory.GetFiles(inputPath, "*.txt").Select(x=>Path.GetFileName(x)).ToArray();
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
            var solver = new CBS.CBSSolver(Path.Combine(inputPath,file));

            var result = solver.RunSearch();
            if (result.Any(x => x.path == null || x.path.Count == 0)) Assert.Fail("path not found");
            
            solver.WriteHumanReadableOutput(Path.Combine(outputPath, file), result);
        }
    }
}
