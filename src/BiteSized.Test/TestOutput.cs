using Environment = System.Environment;
using System.Collections.Generic;
using StringWriter = System.IO.StringWriter;
using NUnit.Framework;

namespace BiteSized.Test
{
    public class OutputTests
    {
        [Test]
        public void TestOK()
        {
            using var writer = new StringWriter();

            var record = new Inspection.Record(
                new List<Inspection.LineTooLong>(),
                10);

            Output.Report("Program.cs", record, 13, 1984, writer);

            var nl = Environment.NewLine;
            Assert.AreEqual($"OK   Program.cs{nl}", writer.ToString());
        }

        [Test]
        public void TestSingleLineTooLong()
        {
            using var writer = new StringWriter();

            var record = new Inspection.Record(
                new List<Inspection.LineTooLong>
                {
                    new Inspection.LineTooLong(3, 123)
                },
                10);

            Output.Report("Program.cs", record, 13, 1984, writer);

            var nl = Environment.NewLine;
            string expected = $"FAIL Program.cs{nl}" +
                              $"  * The following line(s) have more than allowed 13 characters:{nl}" +
                              $"    * Line 3: 123 characters{nl}";


            Assert.AreEqual(expected, writer.ToString());
        }

        [Test]
        public void TestMultipleLinesTooLong()
        {
            using var writer = new StringWriter();

            var record = new Inspection.Record(
                new List<Inspection.LineTooLong>
                {
                    new Inspection.LineTooLong(3, 123),
                    new Inspection.LineTooLong(4, 124)
                },
                10);

            Output.Report("Program.cs", record, 13, 1984, writer);

            var nl = Environment.NewLine;
            string expected = $"FAIL Program.cs{nl}" +
                              $"  * The following line(s) have more than allowed 13 characters:{nl}" +
                              $"    * Line 3: 123 characters{nl}" +
                              $"    * Line 4: 124 characters{nl}";


            Assert.AreEqual(expected, writer.ToString());
        }

        [Test]
        public void TestFileTooLong()
        {
            using var writer = new StringWriter();

            var record = new Inspection.Record(
                new List<Inspection.LineTooLong>(),
                5000);

            Output.Report("Program.cs", record, 13, 1984, writer);

            var nl = Environment.NewLine;
            string expected = $"FAIL Program.cs{nl}" +
                              $"  * The file is too long. It contains 5000 line(s), " +
                              $"but only 1984 line(s) are allowed.{nl}";

            Assert.AreEqual(expected, writer.ToString());
        }

        [Test]
        public void TestLinesAndFileTooLong()
        {
            using var writer = new StringWriter();

            var record = new Inspection.Record(
                new List<Inspection.LineTooLong>
                {
                    new Inspection.LineTooLong(3, 123),
                    new Inspection.LineTooLong(4, 124)
                },
                5000);

            Output.Report("Program.cs", record, 13, 1984, writer);

            var nl = Environment.NewLine;
            string expected = $"FAIL Program.cs{nl}" +
                              $"  * The following line(s) have more than allowed 13 characters:{nl}" +
                              $"    * Line 3: 123 characters{nl}" +
                              $"    * Line 4: 124 characters{nl}" +
                              $"  * The file is too long. It contains 5000 line(s), " +
                              $"but only 1984 line(s) are allowed.{nl}";

            Assert.AreEqual(expected, writer.ToString());
        }
    }
}
