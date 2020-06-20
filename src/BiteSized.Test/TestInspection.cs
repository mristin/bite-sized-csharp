using StringReader = System.IO.StringReader;
using System.Collections.Generic;
using NUnit.Framework;

namespace BiteSizedCsharp.Test
{
    public class InspectionTests
    {
        [Test]
        public void TestEmptyFile()
        {
            using var reader = new StringReader("");
            uint maxLineLength = 120;
            Inspection.Record record = Inspection.InspectLines(reader, maxLineLength);

            Assert.AreEqual(0, record.LinesTooLong.Count);
            Assert.AreEqual(0, record.LineCount);
        }

        [Test]
        public void TestSingleLineWithoutTrailingNewlineOK()
        {
            using var reader = new StringReader("some content");
            uint maxLineLength = 120;
            Inspection.Record record = Inspection.InspectLines(reader, maxLineLength);

            Assert.AreEqual(0, record.LinesTooLong.Count);
            Assert.AreEqual(1, record.LineCount);
        }

        [Test]
        public void TestSingleLineWithTrailingNewlineOK()
        {
            using var reader = new StringReader("some content\n");
            uint maxLineLength = 120;
            Inspection.Record record = Inspection.InspectLines(reader, maxLineLength);

            Assert.AreEqual(0, record.LinesTooLong.Count);
            Assert.AreEqual(1, record.LineCount);
        }

        [Test]
        public void TestSingleLineTooLong()
        {
            using var reader = new StringReader("1234567");
            uint maxLineLength = 3;
            Inspection.Record record = Inspection.InspectLines(reader, maxLineLength);

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
            using var reader = new StringReader("123\n1234\n12345\n123");
            uint maxLineLength = 3;
            Inspection.Record record = Inspection.InspectLines(reader, maxLineLength);

            var expected = new List<Inspection.LineTooLong>
            {
                new Inspection.LineTooLong(2, 4),
                new Inspection.LineTooLong(3, 5)
            };

            Assert.That(record.LinesTooLong, Is.EquivalentTo(expected));
            Assert.AreEqual(4, record.LineCount);
        }

        [Test]
        public void TestWindowsLineCount()
        {
            using var reader = new StringReader("123\r\n123\r\n");
            uint maxLineLength = 3;
            Inspection.Record record = Inspection.InspectLines(reader, maxLineLength);

            Assert.AreEqual(0, record.LinesTooLong.Count);
            Assert.AreEqual(2, record.LineCount);
        }
    }
}
