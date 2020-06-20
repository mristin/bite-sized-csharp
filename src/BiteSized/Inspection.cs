using System;
using System.Collections.Generic;
using TextReader = System.IO.TextReader;

namespace BiteSizedCsharp
{
    public static class Inspection
    {
        public readonly struct LineTooLong
        {
            public readonly ulong LineNumber;
            public readonly uint Length;

            public LineTooLong(ulong lineNumber, uint length)
            {
                if (lineNumber == 0)
                {
                    throw new ArgumentException("Line numbers are indexed from 1, but got: 0");
                }
                LineNumber = lineNumber;
                Length = length;
            }
        }

        public class Record
        {
            public readonly List<LineTooLong> LinesTooLong;
            public readonly ulong LineCount;

            public Record(List<LineTooLong> linesTooLong, ulong lineCount)
            {
                // Precondition(s)
                foreach (var lineTooLong in linesTooLong)
                {
                    if (lineTooLong.LineNumber > lineCount)
                    {
                        throw new ArgumentException(
                            $"Inconsistent line number and line count (== {lineCount}), " +
                            $"got: {lineTooLong.LineNumber}");
                    }
                }

                LinesTooLong = linesTooLong;
                LineCount = lineCount;
            }
        }

        /// <summary>
        /// Checks that the file observes width and height constraints.
        /// </summary>
        /// <param name="reader">Reads the source file</param>
        /// <param name="maxLineLength">maximum allowed line length</param>
        /// <returns>List of problematic comments</returns>
        public static Record InspectLines(TextReader reader, uint maxLineLength)
        {
            var linesTooLong = new List<LineTooLong>(1024);
            ulong lineCount = 0;

            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Length > maxLineLength)
                {
                    linesTooLong.Add(new LineTooLong(lineCount + 1, (uint)line.Length));
                }

                lineCount++;
            }

            return new Record(linesTooLong, lineCount);
        }
    }
}