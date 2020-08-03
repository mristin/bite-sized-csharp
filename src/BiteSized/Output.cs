using System;
using TextWriter = System.IO.TextWriter;

namespace BiteSized
{
    public static class Output
    {
        /// <summary>
        /// Writes the report based on the inspection record.
        /// </summary>
        /// <param name="path">Path to the inspected file</param>
        /// <param name="record">Inspection record of the file</param>
        /// <param name="maxLineLength">Maximum line length used in the inspection</param>
        /// <param name="maxLinesInFile">Maximum number of lines allowed in a file</param>
        /// <param name="writer">Writes the report</param>
        /// <returns>true if no trespasses</returns>
        public static bool Report(string path, Inspection.Record record,
            uint maxLineLength, ulong maxLinesInFile, TextWriter writer)
        {
            ////
            // Precondition(s)
            ////

            foreach (var lineTooLong in record.LinesTooLong)
            {
                if (lineTooLong.Length < maxLineLength)
                {
                    throw new ArgumentException(
                        $"Invalid lineTooLong.Length, " +
                        $"expected smaller than maxLineLength (== {maxLineLength}), " +
                        $"but got: {lineTooLong.Length}");
                }
            }

            ////
            // Implementation
            ////

            if (record.LinesTooLong.Count == 0 && record.LineCount <= maxLinesInFile)
            {
                writer.WriteLine($"OK   {path}");
                return true;
            }

            writer.WriteLine($"FAIL {path}");
            if (record.LinesTooLong.Count > 0)
            {
                writer.WriteLine($"  * The following line(s) have more than allowed {maxLineLength} characters:");
                foreach (var lineTooLong in record.LinesTooLong)
                {
                    writer.WriteLine($"    * Line {lineTooLong.LineNumber}: {lineTooLong.Length} characters");
                }
            }

            if (record.LineCount > maxLinesInFile)
            {
                writer.WriteLine($"  * The file is too long. It contains {record.LineCount} line(s), " +
                                 $"but only {maxLinesInFile} line(s) are allowed.");
            }

            return false;
        }
    }
}