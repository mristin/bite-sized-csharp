using Console = System.Console;
using StreamReader = System.IO.StreamReader;
using File = System.IO.File;
using Environment = System.Environment;
using System.Collections.Generic;

// We can not cherry-pick imports from System.CommandLine since InvokeAsync is a necessary extension.
using System.CommandLine;

namespace BiteSizedCsharp
{
    public class Program
    {
        private static int Handle(string[] inputs, string[] excludes, uint maxLineLength, uint maxLinesInFile)
        {
            int exitCode = 0;

            string cwd = System.IO.Directory.GetCurrentDirectory();
            IEnumerable<string> paths = Input.MatchFiles(
                cwd,
                new List<string>(inputs),
                new List<string>(excludes ?? new string[0]));

            foreach (string path in paths)
            {
                using StreamReader sr = File.OpenText(path);
                var record = Inspection.InspectLines(sr, maxLineLength);

                bool isOk = Output.Report(path, record, maxLineLength, maxLinesInFile, Console.Out);
                if (!isOk)
                {
                    exitCode = 1;
                }
            }

            return exitCode;
        }

        public static int MainWithCode(string[] args)
        {
            var rootCommand = new RootCommand(
                "Checks that the C# code is bite-sized in width (line length) and height (number of lines).")
            {
                new Option<string[]>(
                        new[] {"--inputs", "-i"},
                        "Glob patterns of the files to be inspected")
                    {Required = true},

                new Option<string[]>(
                    new[] {"--excludes", "-e"},
                    "Glob patterns of the files to be excluded from inspection"),

                new Option<uint>(
                    "--max-line-length",
                    () => 120,
                    "Maximum allowed line length"
                ),

                new Option<uint>(
                    "--max-lines-in-file",
                    () => 2000,
                    "Maximum number of lines allowed in a file")
            };

            rootCommand.Handler = System.CommandLine.Invocation.CommandHandler.Create(
                (string[] inputs, string[] excludes, uint maxLineLength, uint maxLinesInFile) =>
                    Handle(inputs, excludes, maxLineLength, maxLinesInFile));

            int exitCode = rootCommand.InvokeAsync(args).Result;
            return exitCode;
        }

        public static void Main(string[] args)
        {
            int exitCode = MainWithCode(args);
            Environment.ExitCode = exitCode;
        }
    }
}