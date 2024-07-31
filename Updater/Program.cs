using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Ionic.Zip;

namespace xp_apps.Updater
{
    // CurlWrapperL - lite without logger and other trash
    public abstract class CurlWrapperL
    {
        private const int ProgressBarWidth = 30;
        private static readonly string Curl = GetCurl(GetCommandArgs());

        private static string GetArgValue(string[] args, string key)
        {
            var arg = args.FirstOrDefault(a => a.StartsWith(key + "=", StringComparison.OrdinalIgnoreCase));
            return arg?.Substring(key.Length + 1);
        }

        private static string GetCurl(string[] args)
        {
            // method 1 - search in program arguments
            var curlFromArgs = GetArgValue(args, "--curl");
            if (!string.IsNullOrEmpty(curlFromArgs)) return curlFromArgs;


            // method 2 - search in environment variable
            var curlPath = Environment.GetEnvironmentVariable("CURL_PATH");

            if (!string.IsNullOrEmpty(curlPath) || File.Exists(curlPath)) return curlPath;

            // method 3 - search in curl/curl.exe folder
            if (Directory.Exists(Directory.GetCurrentDirectory() + "\\curl"))
            {
                var curlFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "curl", "curl.exe");

                if (File.Exists(curlFolderPath)) return curlFolderPath;
            }

            Console.WriteLine("Could not find curl. Exiting");
            Console.ReadLine();
            Environment.Exit(0);

            return null;
        }

        public static void DownloadFile(string url, string filename)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = Curl,
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
                    if (progress >= 0 && lastProgress != line)
                    {
                        lastProgress = line;
                        DisplayProgress(progress, stopwatch);
                    }
                    else if (line.Contains("Could not resolve host"))
                    {
                        Console.WriteLine(
                            $"Could not download {filename}\nUnable to resolve host. Are you sure you entered the correct URL?");
                    }
                    else if (line.Contains("timeout"))
                    {
                        Console.WriteLine($"Could not download {filename}\nRequest timed out. Is the website alive?\n");
                    }
                }

                stopwatch.Stop();
                Thread.Sleep(1000);

                string completionLine = null;
                try
                {
                    completionLine = outputReader.ReadToEnd();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading completion output: {ex.Message}");
                }

                if (!string.IsNullOrEmpty(completionLine))
                {
                    Console.WriteLine($"\nError: {completionLine}\n");
                }

                Console.WriteLine($"\n\n{filename} download completed.\n");
            }
        }

        private static float ParseProgress(string line)
        {
            var match = Regex.Match(line, @"(\d+[\.,]?\d*)%\s*");

            if (match.Success && float.TryParse(match.Groups[1].Value.Replace(",", "."), NumberStyles.Float,
                    CultureInfo.InvariantCulture, out var progress)) return progress;

            return -1;
        }

        private static void DisplayProgress(float progress, Stopwatch stopwatch)
        {
            if (progress <= 0 || stopwatch.Elapsed.TotalSeconds <= 0) return;

            var speed = progress / stopwatch.Elapsed.TotalSeconds;
            var remainingPercent = 100 - progress;
            var remainingSeconds = remainingPercent / speed;
            var remainingTime = remainingSeconds < TimeSpan.MaxValue.TotalSeconds
                ? TimeSpan.FromSeconds(remainingSeconds)
                : TimeSpan.MaxValue;


            var progressChars = (int)(ProgressBarWidth * (progress / 100.0));
            var progressBar = new string('#', progressChars) + new string('-', ProgressBarWidth - progressChars);

            var progressText =
                $"\r[{progressBar}] {progress:0.00}% | {speed:0.00} MB/s | {remainingTime:hh\\:mm\\:ss} remaining";

            Console.Write(progressText);
        }

        private static string[] GetCommandArgs()
        {
            return Environment.GetCommandLineArgs().Skip(1).ToArray();
        }
    }

    internal abstract class Updater
    {
        private static void Main()
        {
            Thread.Sleep(2000);
            
            CurlWrapperL.DownloadFile(sources.Updater.LatestReleaseZip, "xp-apps.zip");

            using (var zipFile = new ZipFile("xp-apps.zip"))
            {
                foreach (var entry in zipFile)
                {
                    Console.WriteLine(entry.FileName);
                    if (!entry.FileName.Equals("Updater.exe", StringComparison.OrdinalIgnoreCase) &&
                        !entry.FileName.Equals("DotNetZip.dll", StringComparison.OrdinalIgnoreCase))
                    {
                        entry.Extract(".", ExtractExistingFileAction.OverwriteSilently);
                    }
                    else Console.WriteLine("Skipping Updater.exe");
                }
            }

#warning TODO: Rename original exe file if user changed (why not?)
            File.Delete("xp-apps.zip");

            Console.WriteLine("Application has been updated successfully.");
            Console.ReadLine();
            Environment.Exit(0);
        }
    }
}