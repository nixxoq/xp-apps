using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using xp_apps.sources;

namespace CurlWrapper
{
    internal abstract class Program
    {
        private static readonly char[] AnimationChars = { '/', '-', '\\', '|' };
        private static int _animationIndex;
        private static int _progressTopPosition;
        private const string ProgramVersion = "1.0.0.0";

        public static void Main()
        {
            SimpleLogger.SetupLog("CurlWrapper");
            var args = Helper.GetCommandArgs();

            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }

            foreach (var arg in args)
            {
                switch (arg)
                {
                    case "--check":
#warning TODO: Implement checker
                        GetCurl();
                        break;
                    case "-h":
                    case "--help":
                        ShowHelp();
                        break;
                }
            }

            var curl = GetArgValue(args, "--curl");
            var url = GetArgValue(args, "--url");
            var filename = GetArgValue(args, "--filename");

            if (string.IsNullOrEmpty(curl))
            {
                SimpleLogger.Logger.Info(
                    "Could not find the path to curl in the program arguments. Trying to find in System Variables...");
                curl = GetCurl();
            }

            if (string.IsNullOrEmpty(url))
            {
                Console.WriteLine("Error: --url parameter is required.");
                Environment.Exit(-1);
            }

            if (string.IsNullOrEmpty(filename)) filename = Helper.ExtractFileNameFromUrl(url);

            Console.WriteLine($"Starting download: {filename}");
            _progressTopPosition = Console.CursorTop;
            DownloadFile(curl, url, filename);
        }

        private static string GetArgValue(string[] args, string key)
        {
            var arg = args.FirstOrDefault(a => a.StartsWith(key + "=", StringComparison.OrdinalIgnoreCase));
            return arg?.Substring(key.Length + 1);
        }

        private static string GetCurl()
        {
            var pathEnv = Environment.GetEnvironmentVariable("PATH")?.Split(';');
            if (pathEnv != null)
            {
                var curlPaths = pathEnv
                    .Where(p => p.IndexOf("curl", StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToArray();

                if (curlPaths.Any()) return curlPaths.First();

                SimpleLogger.Logger.Debug("Could not find curl in the PATH environment. Exiting");
                Environment.Exit(-1);
            }
            else
            {
                SimpleLogger.Logger.Debug("The PATH environment variable is not found or empty. Exiting");
                Environment.Exit(-1);
            }

            return null;
        }

        private static void DownloadFile(string curlPath, string url, string filename)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = curlPath,
                Arguments = $"-Lo \"{filename}\" \"{url}\" --progress-bar",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8
            };

            using (var process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();

                var errorReader = process.StandardError;
                var outputReader = process.StandardOutput;
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                string lastProgress = null;

                while (!process.HasExited)
                {
                    var line = errorReader.ReadLine();
                    if (line == null) continue;

                    var progress = ParseProgress(line);
                    if (!(progress >= 0) || lastProgress == line) continue;

                    lastProgress = line;
                    DisplayProgress(filename, progress, stopwatch);
                }

                stopwatch.Stop();
                Thread.Sleep(1000);

                var completionLine = outputReader.ReadToEnd();
                Console.WriteLine(string.IsNullOrEmpty(completionLine)
                    ? $"\n{filename} download completed.\n"
                    : $"\nError: {completionLine}\n");
            }
        }

        private static float ParseProgress(string line)
        {
            var match = Regex.Match(line, @"(\d+\.\d+)%");
            if (match.Success && float.TryParse(match.Groups[1].Value, out var progress)) return progress;

            return -1; // Return -1 if no progress percentage could be parsed
        }

        private static void DisplayProgress(string filename, float progress, Stopwatch stopwatch)
        {
            if (progress <= 0 || stopwatch.Elapsed.TotalSeconds <= 0) return;

            var speed = progress / stopwatch.Elapsed.TotalSeconds;
            var remainingPercent = 100 - progress;
            var remainingSeconds = remainingPercent / speed;
            var remainingTime = remainingSeconds < TimeSpan.MaxValue.TotalSeconds
                ? TimeSpan.FromSeconds(remainingSeconds)
                : TimeSpan.MaxValue;

            var animationChar = AnimationChars[_animationIndex++ % AnimationChars.Length];

            var originalTop = Console.CursorTop;
            var originalLeft = Console.CursorLeft;

            Console.SetCursorPosition(0, _progressTopPosition);
            Console.Write(
                $@"{animationChar} Downloading {filename} | {progress:0.00}% completed | {remainingTime:hh\:mm\:ss} remaining");
            Console.SetCursorPosition(originalLeft, originalTop);
        }

        private static void ShowHelp()
        {
            Console.WriteLine($"CurlWrapper version {ProgramVersion}" +
                              "\n\nList of available arguments:\n\n[Option]\t\t\t\t[Description]" +
                              "\n-h, --help\t\t\t\tDisplay this help message" +
                              "\n--check\t\t\t\t\tCheck if CURL exists in system" +
                              "\n--curl\t\t\t\t\tSpecify own CURL.exe to use" +
                              "\n--url\t\t\t\t\tUrl to download" +
                              $"\n\nExamples:\n   {Helper.CurrentFile} --check" +
                              $"\n   {Helper.CurrentFile} --curl=curl.exe --url=https://example.com/file.txt");
        }
    }
}