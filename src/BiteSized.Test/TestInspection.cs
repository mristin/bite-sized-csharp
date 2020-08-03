using StringReader = System.IO.StringReader;
using Regex = System.Text.RegularExpressions.Regex;

using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

namespace BiteSized.Test
{
    public class InspectionTests
    {
        [Test]
        public void TestEmptyFile()
        {
            using var reader = new StringReader("");
            uint maxLineLength = 120;
            var ignoreLinesMatching = new List<Regex>();
            Inspection.Record record = Inspection.InspectLines(reader, maxLineLength, ignoreLinesMatching);

            Assert.AreEqual(0, record.LinesTooLong.Count);
            Assert.AreEqual(0, record.LineCount);
        }

        [Test]
        public void TestSingleLineWithoutTrailingNewlineOK()
        {
            using var reader = new StringReader("some content");
            uint maxLineLength = 120;
            var ignoreLinesMatching = new List<Regex>();
            Inspection.Record record = Inspection.InspectLines(reader, maxLineLength, ignoreLinesMatching);

            Assert.AreEqual(0, record.LinesTooLong.Count);
            Assert.AreEqual(1, record.LineCount);
        }

        [Test]
        public void TestSingleLineWithTrailingNewlineOK()
        {
            var nl = System.Environment.NewLine;
            using var reader = new StringReader($"some content{nl}");
            uint maxLineLength = 120;
            var ignoreLinesMatching = new List<Regex>();
            Inspection.Record record = Inspection.InspectLines(reader, maxLineLength, ignoreLinesMatching);

            Assert.AreEqual(0, record.LinesTooLong.Count);
            Assert.AreEqual(1, record.LineCount);
        }

        [Test]
        public void TestSingleLineTooLong()
        {
            using var reader = new StringReader("1234567");
            uint maxLineLength = 3;
            var ignoreLinesMatching = new List<Regex>();
            Inspection.Record record = Inspection.InspectLines(reader, maxLineLength, ignoreLinesMatching);

            var expected = new List<Inspection.LineTooLong>
            {
                new Inspection.LineTooLong(1, 7)
            };

            Assert.That(record.LinesTooLong, Is.EquivalentTo(expected));
            Assert.AreEqual(1, record.LineCount);
        }

        [Test]
        public void TestMultipleTrespassingLines()
        {
            var nl = System.Environment.NewLine;
            using var reader = new StringReader($"123{nl}1234{nl}12345{nl}123");
            uint maxLineLength = 3;
            var ignoreLinesMatching = new List<Regex>();
            Inspection.Record record = Inspection.InspectLines(reader, maxLineLength, ignoreLinesMatching);

            var expected = new List<Inspection.LineTooLong>
            {
                new Inspection.LineTooLong(2, 4),
                new Inspection.LineTooLong(3, 5)
            };

            Assert.That(record.LinesTooLong, Is.EquivalentTo(expected));
            Assert.AreEqual(4, record.LineCount);
        }

        [Test]
        public void TestIgnoreLinesMatching()
        {
            var nl = System.Environment.NewLine;

            var testCases = new List<(string, string[], ulong[], string)>
            {
                ("AA!?", new[] {"AA"}, new ulong[]{},
                    "line ignored"),

                ($"AA--{nl}#@!?", new[] {"AA"}, new ulong[]{2},
                    "one line ignored, other hits"),

                ($"AA!?{nl}BB!?{nl}#@!?", new[] {"AA", "BB"}, new ulong[]{3},
                    "ignored lines should match at least one pattern, one line still hits")
            };

            foreach (var (text, patterns, expectedLineTooLongs, caseName) in
                testCases)
            {
                using var reader = new StringReader(text);
                uint maxLineLength = 3;

                var ignoreLinesMatching =
                    patterns
                    .Select(p => new Regex(p))
                    .ToList();

                Inspection.Record record = Inspection.InspectLines(reader, maxLineLength, ignoreLinesMatching);

                Assert.That(
                    record.LinesTooLong.Select(l => l.LineNumber).ToList(),
                    Is.EquivalentTo(expectedLineTooLongs),
                    caseName);
            }
        }

        [Test]
        public void TestWindowsLineCount()
        {
            using var reader = new StringReader("123\r\n123\r\n");
            uint maxLineLength = 3;
            var ignoreLinesMatching = new List<Regex>();
            Inspection.Record record = Inspection.InspectLines(reader, maxLineLength, ignoreLinesMatching);

            Assert.AreEqual(0, record.LinesTooLong.Count);
            Assert.AreEqual(2, record.LineCount);
        }

        [Test]
        public void TestLinuxLineCount()
        {
            using var reader = new StringReader("123\n123\n");
            uint maxLineLength = 3;
            var ignoreLinesMatching = new List<Regex>();
            Inspection.Record record = Inspection.InspectLines(reader, maxLineLength, ignoreLinesMatching);

            Assert.AreEqual(0, record.LinesTooLong.Count);
            Assert.AreEqual(2, record.LineCount);
        }
    }
}
