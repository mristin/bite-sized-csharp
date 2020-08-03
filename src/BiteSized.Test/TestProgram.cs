using Path = System.IO.Path;
using Environment = System.Environment;
using NUnit.Framework;

namespace BiteSized.Test
{
    public class ProgramTests
    {
        [Test]
        public void TestNoCommandLineArguments()
        {
            using var consoleCapture = new ConsoleCapture();

            int exitCode = Program.MainWithCode(new string[0]);

            string nl = Environment.NewLine;

            Assert.AreEqual(1, exitCode);
            Assert.AreEqual($"Option '--inputs' is required.{nl}{nl}", consoleCapture.Error());
        }

        [Test]
        public void TestInvalidCommandLineArguments()
        {
            using var consoleCapture = new ConsoleCapture();

            int exitCode = Program.MainWithCode(new[] { "--invalid-arg" });

            string nl = Environment.NewLine;

            Assert.AreEqual(1, exitCode);
            Assert.AreEqual(
                $"Option '--inputs' is required.{nl}" +
                $"Unrecognized command or argument '--invalid-arg'{nl}{nl}",
                consoleCapture.Error());
        }

        [Test]
        public void TestInputOk()
        {
            using var tmpdir = new TemporaryDirectory();

            string path = Path.Join(tmpdir.Path, "SomeProgram.cs");

            System.IO.File.WriteAllText(path, "123\n");

            using var consoleCapture = new ConsoleCapture();

            int exitCode = Program.MainWithCode(new[] { "--inputs", path });

            string nl = Environment.NewLine;

            Assert.AreEqual(0, exitCode);
            Assert.AreEqual(
                $"OK   {path}{nl}",
                consoleCapture.Output());
        }

        [Test]
        public void TestInputFail()
        {
            using var tmpdir = new TemporaryDirectory();

            string path = Path.Join(tmpdir.Path, "SomeProgram.cs");

            System.IO.File.WriteAllText(path, "12345\n123456\n");

            using var consoleCapture = new ConsoleCapture();

            int exitCode = Program.MainWithCode(
                new[] { "--inputs", path, "--max-line-length", "3", "--max-lines-in-file", "1" });

            string nl = Environment.NewLine;

            Assert.AreEqual(1, exitCode);

            Assert.AreEqual(
                $"FAIL {path}{nl}" +
                $"  * The following line(s) have more than allowed 3 characters:{nl}" +
                $"    * Line 1: 5 characters{nl}" +
                $"    * Line 2: 6 characters{nl}" +
                $"  * The file is too long. It contains 2 line(s), but only 1 line(s) are allowed.{nl}",
                consoleCapture.Output());

            Assert.AreEqual($"One or more input files failed the checks.{nl}", consoleCapture.Error());
        }

        [Test]
        public void TestExcludes()
        {
            using var tmpdir = new TemporaryDirectory();

            string pathExcluded = Path.Join(tmpdir.Path, "ExcludedProgram.cs");
            System.IO.File.WriteAllText(pathExcluded, "12345\n123456\n");

            string pathIncluded = Path.Join(tmpdir.Path, "IncludedProgram.cs");
            System.IO.File.WriteAllText(pathIncluded, "123\n");

            using var consoleCapture = new ConsoleCapture();

            int exitCode = Program.MainWithCode(
                new[] {
                    "--inputs", Path.Join(tmpdir.Path, "Included*.cs"),
                    "--excludes", Path.Join(tmpdir.Path, "Excluded*.cs"),
                    "--max-line-length", "3", "--max-lines-in-file", "1" });

            string nl = Environment.NewLine;

            Assert.AreEqual(0, exitCode);

            Assert.AreEqual("", consoleCapture.Error());

            Assert.AreEqual(
                $"OK   {pathIncluded}{nl}",
                consoleCapture.Output());
        }

        [Test]
        public void TestIgnoreLinesMatching()
        {
            using var tmpdir = new TemporaryDirectory();

            string path = Path.Join(tmpdir.Path, "SomeProgram.cs");

            System.IO.File.WriteAllText(path, "12345\nAAA 123456\n123456\n");

            using var consoleCapture = new ConsoleCapture();

            int exitCode = Program.MainWithCode(
                new[]
                {
                    "--inputs", path,
                    "--max-line-length", "3",
                    "--ignore-lines-matching", "^AAA"
                });

            string nl = Environment.NewLine;

            Assert.AreEqual(1, exitCode);

            Assert.AreEqual(
                $"FAIL {path}{nl}" +
                $"  * The following line(s) have more than allowed 3 characters:{nl}" +
                $"    * Line 1: 5 characters{nl}" +
                $"    * Line 3: 6 characters{nl}" +
                $"The --ignore-lines-matching was set to: ^AAA{nl}",
                consoleCapture.Output());

            Assert.AreEqual($"One or more input files failed the checks.{nl}", consoleCapture.Error());
        }
    }
}
