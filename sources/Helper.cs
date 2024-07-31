using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace xp_apps.sources
{
    public abstract class Helper
    {
        ////////////////////////
        /// HELPER FUNCTIONS ///
        ////////////////////////
        public static readonly string OsArchitecture = Environment.Is64BitOperatingSystem ? "x64" : "x86";

        public static readonly string WorkDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static readonly string CurrentFile = AppDomain.CurrentDomain.FriendlyName;

        public static string ExtractFileNameFromUrl(string url) =>
            url.Substring(url.LastIndexOf('/') + 1);

        public static void CreateApplicationFolder(string folderPath = null)
        {
            if (folderPath == null) folderPath = Path.Combine(WorkDir, Cache.CacheFolder);

            if (Directory.Exists(folderPath)) return;

            try
            {
                Directory.CreateDirectory(folderPath);
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access denied to creating folder {folderPath}");
            }
            catch (PathTooLongException)
            {
                Console.WriteLine(
                    $"Path too long to create folder {folderPath}. Place xp-apps in a different folder with a shorter path.");
            }
        }

        ///////////////
        /// NETWORK ///
        ///////////////
        public static bool IsNetworkAvailable()
        {
            var ping = new Ping();
            var reply = ping.Send("www.google.com", 5000);

            return reply != null && reply.Status == IPStatus.Success;
        }

        public static WebClient GetClient()
        {
            var client = new WebClient();
            return client;
        }

        /// <summary>
        ///     Downloads a file from the specified URL and saves it with the given filename.
        ///     Displays a progress animation in the console while downloading.
        /// </summary>
        /// <param name="url">The URL of the file to download.</param>
        /// <param name="filename">The name of the file to save the downloaded content to.</param>
        public static void DownloadFile(string url, string filename)
        {
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                CurlWrapper.DownloadFile(url, filename);
                return;
            }

            using (var client = new WebClient())
            {
                char[] animationChars = { '/', '-', '\\', '|' };
                var animationIndex = 0;
                var stopwatch = new Stopwatch();

                client.DownloadProgressChanged += (sender, e) =>
                {
                    var speed = e.BytesReceived / 1024d / stopwatch.Elapsed.TotalSeconds;
                    var remainingBytes = e.TotalBytesToReceive - e.BytesReceived;
                    var remainingSeconds = remainingBytes / 1024d / speed;

                    var remainingTime = TimeSpan.FromSeconds(remainingSeconds);
                    var animationChar = animationChars[animationIndex++ % animationChars.Length];

                    Console.Write(
                        $"\r{animationChar} " +
                        $@"Downloading {filename} | {e.ProgressPercentage}% completed | {speed / 1024d:0.00} MB/s | {remainingTime:hh\:mm\:ss} remaining"
                    );
                };

                client.DownloadFileCompleted += (sender, e) =>
                {
                    Thread.Sleep(1000);
                    Console.WriteLine(e.Error != null
                        ? $"\nError: {e.Error.Message}"
                        : $"\n{filename} download completed.\n");
                };

                stopwatch.Start();
                client.DownloadFileAsync(new Uri(url), filename);

                while (client.IsBusy) Thread.Sleep(100);

                stopwatch.Stop();
            }
        }

        public static string[] GetCommandArgs()
        {
            return Environment.GetCommandLineArgs().Skip(1).ToArray();
        }
    }

    public abstract class CurlWrapper
    {
        private const int ProgressBarWidth = 30;
        private static readonly string Curl = GetCurl(Helper.GetCommandArgs());

        private static string GetArgValue(string[] args, string key)
        {
            var arg = args.FirstOrDefault(a => a.StartsWith(key + "=", StringComparison.OrdinalIgnoreCase));
            return arg?.Substring(key.Length + 1);
        }

        private static string GetCurl(string[] args)
        {
            // method 1 - search in program arguments
            var curlFromArgs = GetArgValue(args, "--curl");
            if (!string.IsNullOrEmpty(curlFromArgs))
            {
                SimpleLogger.Logger.Info("Picked up curl from command-line arguments");
                return curlFromArgs;
            }

            // method 2 - search in environment variable
            var curlPath = Environment.GetEnvironmentVariable("CURL_PATH");

            if (string.IsNullOrEmpty(curlPath) || !File.Exists(curlPath))
                SimpleLogger.Logger.Info(
                    "Could not find curl in the CURL_PATH environment variable or the file does not exist.");
            else
            {
                SimpleLogger.Logger.Info("Picked up curl from the CURL_PATH environment");
                return curlPath;
            }

            // method 3 - search in curl/curl.exe folder
            if (Directory.Exists(Helper.WorkDir + "\\curl"))
            {
                var curlFolderPath = Path.Combine(Helper.WorkDir, "curl", "curl.exe");

                if (File.Exists(curlFolderPath))
                {
                    return curlFolderPath;
                }
            }

            Console.WriteLine("Could not find curl. Exiting");
            SimpleLogger.Logger.Info("Could not find curl. Exiting");
            Environment.Exit(0);

            return null;
        }

        public static string GetFileContent(string url)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = Curl,
                Arguments = $"-s \"{url}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();

                string content = null;
                string error = null;

                try
                {
                    content = process.StandardOutput.ReadToEnd();
                    error = process.StandardError.ReadToEnd();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading process output: {ex.Message}");
                }

                process.WaitForExit();

                if (process.ExitCode == 0) return content;
                
                Console.WriteLine($"Error: {error}");
                return null;

            }
        }

        public static string GetFileSize(string url)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = Curl,
                Arguments = $"-I \"{url}\" --silent",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();

                string content = null;
                string error = null;

                try
                {
                    content = process.StandardOutput.ReadToEnd();
                    error = process.StandardError.ReadToEnd();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading process output: {ex.Message}");
                }

                process.WaitForExit();

                if (process.ExitCode != 0) Console.WriteLine($"Error: {error}");

                var contentLengthLine = content?.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .FirstOrDefault(line => line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase));

                var contentLength = contentLengthLine?.Split(':')[1].Trim();
                return contentLength;
            }
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
                        SimpleLogger.Logger.Info(
                            $"Could not download {filename}\n    -> Unable to resolve host. Are you sure you entered the correct URL?\n");
                    }
                    else if (line.Contains("timeout"))
                    {
                        Console.WriteLine($"Could not download {filename}\nRequest timed out. Is the website alive?\n");
                        SimpleLogger.Logger.Info(
                            $"Could not download {filename}\n    -> Request timed out. Is the website alive?");
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
            // if (!string.IsNullOrEmpty(filename))
            // {
            //     Console.WriteLine($" | {filename}");
            // }
        }
    }
}