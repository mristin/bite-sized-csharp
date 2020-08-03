using Console = System.Console;
using StreamReader = System.IO.StreamReader;
using File = System.IO.File;
using Environment = System.Environment;
using Regex = System.Text.RegularExpressions.Regex;

using System.Collections.Generic;

// We can not cherry-pick imports from System.CommandLine since InvokeAsync is a necessary extension.
using System.CommandLine;
using System.Linq;

namespace BiteSized
{
    public class Program
    {
        // ReSharper disable once ClassNeverInstantiated.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public class Arguments
        {
#pragma warning disable 8618
            // ReSharper disable UnusedAutoPropertyAccessor.Global
            // ReSharper disable CollectionNeverUpdated.Global
            public string[] Inputs { get; set; }
            public string[]? Excludes { get; set; }
            public string[]? IgnoreLinesMatching { get; set; }
            public uint MaxLineLength { get; set; }
            public uint MaxLinesInFile { get; set; }
            // ReSharper restore CollectionNeverUpdated.Global
            // ReSharper restore UnusedAutoPropertyAccessor.Global
#pragma warning restore 8618
        }

        private static int Handle(Arguments a)
        {
            int exitCode = 0;

            string cwd = System.IO.Directory.GetCurrentDirectory();
            IEnumerable<string> paths = Input.MatchFiles(
                cwd,
                new List<string>(a.Inputs),
                new List<string>(a.Excludes ?? new string[0]));

            var ignoreLinesMatching = new List<Regex>(a.IgnoreLinesMatching?.Length ?? 0);
            foreach (var pattern in a.IgnoreLinesMatching ?? new string[] { })
            {
                try
                {
                    var regex = new Regex(pattern);
                    ignoreLinesMatching.Add(regex);
                }
                catch (System.Exception ex)
                {
                    Console.Error.WriteLine($"Failed to parse the regular expression {pattern}: {ex}");
                }
            }

            bool success = true;
            foreach (string path in paths)
            {
                using StreamReader sr = File.OpenText(path);
                var record = Inspection.InspectLines(sr, a.MaxLineLength, ignoreLinesMatching);

                bool isOk = Output.Report(path, record, a.MaxLineLength, a.MaxLinesInFile, Console.Out);
                success = success & isOk;
            }

            if (!success)
            {
                if (ignoreLinesMatching.Count > 0)
                {
                    Console.Out.WriteLine(
                        $"The --ignore-lines-matching was set to: " +
                        string.Join(" ", ignoreLinesMatching.Select(i => i.ToString())));
                }
                Console.Error.WriteLine("One or more input files failed the checks.");

                exitCode = 1;
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

                new Option<string[]?>(
                    new[] {"--excludes", "-e"},
                    "Glob patterns of the files to be excluded from inspection"),

                new Option<string[]?>(
                    new []{"--ignore-lines-matching"},
                    "Ignore lines matching the regular expression(s)"),

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

            rootCommand.Handler = System.CommandLine.Invocation.CommandHandler.Create((Arguments a) => Handle(a));

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