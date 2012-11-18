// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimeRecorder.cs" company="objo.net">
//   The MIT License (MIT)
//   
//   Copyright (c) 2012 Oystein Bjorke
//   
//   Permission is hereby granted, free of charge, to any person obtaining a
//   copy of this software and associated documentation files (the
//   "Software"), to deal in the Software without restriction, including
//   without limitation the rights to use, copy, modify, merge, publish,
//   distribute, sublicense, and/or sell copies of the Software, and to
//   permit persons to whom the Software is furnished to do so, subject to
//   the following conditions:
//   
//   The above copyright notice and this permission notice shall be included
//   in all copies or substantial portions of the Software.
//   
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//   OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//   MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//   IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//   CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//   TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//   SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
// <summary>
//   Implements the time recording loop.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace TimeRecorder
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     Implements the time recording loop.
    /// </summary>
    public static class TimeRecorder
    {
        /// <summary>
        ///     The log period (in seconds)
        /// </summary>
        private const int LogPeriod = 60;

        /// <summary>
        ///     The path environment variable
        /// </summary>
        private const string PathEnvironmentVariable = "TIMERECORDER";

        /// <summary>
        ///     The default folder
        /// </summary>
        private const string DefaultFolder = "TimeRecorder";

        /// <summary>
        ///     The categories file
        /// </summary>
        private const string CategoriesFile = "categories.txt";

        /// <summary>
        ///     Initializes static members of the <see cref="TimeRecorder" /> class.
        /// </summary>
        static TimeRecorder()
        {
            RootFolder = GetRootFolder();
            CreateDirectory(RootFolder);
            CategoriesPath = Path.Combine(RootFolder, CategoriesFile);
            Folder = Path.Combine(RootFolder, Environment.MachineName);
        }

        /// <summary>
        ///     Gets the root folder.
        /// </summary>
        public static string RootFolder { get; private set; }

        /// <summary>
        ///     Gets the database folder for this machine (root folder + machine name).
        /// </summary>
        public static string Folder { get; private set; }

        /// <summary>
        ///     Gets the categories path.
        /// </summary>
        public static string CategoriesPath { get; private set; }

        /// <summary>
        ///     Runs the time recording asynchronously.
        /// </summary>
        /// <returns>The cancellation token.</returns>
        public static CancellationTokenSource RunAsync()
        {
            var source = new CancellationTokenSource();
            Task.Factory.StartNew(() => Run(source.Token));
            return source;
        }

        /// <summary>
        /// Formats the file name for the specified date.
        /// </summary>
        /// <param name="date">
        /// The date.
        /// </param>
        /// <returns>
        /// The file name.
        /// </returns>
        public static string FormatFileName(DateTime date)
        {
            return date.ToString("yyyy-MM-dd") + ".txt";
        }

        /// <summary>
        ///     Loads the categories from the configuration file in the root folder.
        /// </summary>
        /// <returns>The categories.</returns>
        public static IEnumerable<string> LoadCategories()
        {
            var categories = new List<string>();
            if (File.Exists(CategoriesPath))
            {
                var lines = File.ReadAllLines(CategoriesPath);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    categories.Add(line.Trim());
                }
            }

            return categories;
        }

        /// <summary>
        /// Executes the time recording until the token is canceled.
        /// </summary>
        /// <param name="token">
        /// The cancellation token.
        /// </param>
        private static void Run(CancellationToken token)
        {
            if (!File.Exists(CategoriesPath))
            {
                // create a default categories file
                File.WriteAllLines(CategoriesPath, new[] { "gmail", "Visual Studio" });
            }

            CreateDirectory(Folder);
            Debug.WriteLine("Logging to " + Folder);
            Log("Opened " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            var previousPosition = new Point();
            var previousTime = DateTime.Now;
            bool hasMoved = false;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var current = GetCursorPos();

                    // Log(folder, current.X + "," + current.Y);
                    hasMoved |= current.X != previousPosition.X || current.Y != previousPosition.Y;
                    previousPosition = current;

                    var now = DateTime.Now;
                    if (hasMoved && now - previousTime > TimeSpan.FromSeconds(LogPeriod))
                    {
                        var p = GetForegroundWindowProcess();
                        var moduleName = p != null ? p.Modules[0].ModuleName : "n/a";
                        var title = GetForegroundWindowTitle();
                        var availableCategories = LoadCategories();
                        var categories = GetCategories(title + " " + moduleName, availableCategories);
                        var text = string.Format("{0:00}:{1:00};{2}", now.Hour, now.Minute, categories);
                        var path = Path.Combine(Folder, FormatFileName(now));
                        using (var f = File.AppendText(path))
                        {
                            f.WriteLine(text);
                        }

                        Console.WriteLine(text);
                        previousTime = now;
                        hasMoved = false;
                    }
                }
                catch (Exception e)
                {
                    Log(e.Message);
                }

                Thread.Sleep(2000);
            }

            Log("Closed " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        /// <summary>
        /// Writes the specified text to the log file.
        /// </summary>
        /// <param name="text">
        /// The text.
        /// </param>
        private static void Log(string text)
        {
            var logpath = Path.Combine(Folder, "log.txt");
            using (var f = File.AppendText(logpath))
            {
                f.WriteLine(text);
            }
        }

        /// <summary>
        /// Gets the categories for the specified title.
        /// </summary>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="availableCategories">
        /// The available categories.
        /// </param>
        /// <returns>
        /// A string containing the categories separated by '|'.
        /// </returns>
        private static string GetCategories(string title, IEnumerable<string> availableCategories)
        {
            if (availableCategories == null)
            {
                return string.Empty;
            }

            var cat = string.Empty;
            foreach (var c in availableCategories)
            {
                if (string.IsNullOrWhiteSpace(c))
                {
                    continue;
                }

                if (title.IndexOf(c, StringComparison.InvariantCultureIgnoreCase) < 0)
                {
                    continue;
                }

                if (cat.Length > 0)
                {
                    cat += "|";
                }

                cat += c;
            }

            return cat;
        }

        /// <summary>
        /// Creates the directory (and its ancestor folders).
        /// </summary>
        /// <param name="folder">
        /// The folder.
        /// </param>
        private static void CreateDirectory(string folder)
        {
            var p = string.Empty;
            foreach (var part in folder.Split('\\'))
            {
                p += part + "\\";
                if (!Directory.Exists(p))
                {
                    Directory.CreateDirectory(p);
                }
            }
        }

        /// <summary>
        ///     Gets the root folder of the logs.
        /// </summary>
        /// <returns>The root folder.</returns>
        private static string GetRootFolder()
        {
            return Environment.GetEnvironmentVariable(PathEnvironmentVariable)
                   ?? Path.Combine(GetDropboxFolder(), DefaultFolder);
        }

        /// <summary>
        ///     Gets the Dropbox folder.
        /// </summary>
        /// <returns>The Dropbox folder.</returns>
        private static string GetDropboxFolder()
        {
            var dropboxPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dropbox\\host.db");
            var lines = File.ReadAllLines(dropboxPath);
            var dropboxBase64Text = Convert.FromBase64String(lines[1]);
            return Encoding.ASCII.GetString(dropboxBase64Text);
        }

        /// <summary>
        /// Gets the cursor position.
        /// </summary>
        /// <param name="point">
        /// The position.
        /// </param>
        /// <returns>
        /// True if the position was found.
        /// </returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out Point point);

        /// <summary>
        ///     Gets the foreground window.
        /// </summary>
        /// <returns>The window handle.</returns>
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        /// <summary>
        /// Gets the window text.
        /// </summary>
        /// <param name="hwnd">
        /// The window handle.
        /// </param>
        /// <param name="text">
        /// The text.
        /// </param>
        /// <param name="maxCount">
        /// The size of the text.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hwnd, StringBuilder text, int maxCount);

        /// <summary>
        /// Gets the length of the window text.
        /// </summary>
        /// <param name="hwnd">
        /// The window handle.
        /// </param>
        /// <returns>
        /// The length
        /// </returns>
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowTextLength(IntPtr hwnd);

        /// <summary>
        ///     Gets the cursor position.
        /// </summary>
        /// <returns>The point</returns>
        private static Point GetCursorPos()
        {
            Point pt;
            GetCursorPos(out pt);
            return pt;
        }

        /// <summary>
        ///     Gets the title of the foreground window.
        /// </summary>
        /// <returns>The title.</returns>
        private static string GetForegroundWindowTitle()
        {
            var windowHandle = GetForegroundWindow();
            int length = GetWindowTextLength(windowHandle);
            var sb = new StringBuilder(length + 1);
            GetWindowText(windowHandle, sb, sb.Capacity);
            return sb.ToString();
        }

        /// <summary>
        ///     Gets the process of the foreground window.
        /// </summary>
        /// <returns>The process.</returns>
        private static Process GetForegroundWindowProcess()
        {
            var windowHandle = GetForegroundWindow();
            return Process.GetProcesses().FirstOrDefault(p => p.MainWindowHandle == windowHandle);
        }

        /// <summary>
        ///     The point.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct Point
        {
            /// <summary>
            ///     The x coordinate.
            /// </summary>
            public readonly int X;

            /// <summary>
            ///     The y coordinate.
            /// </summary>
            public readonly int Y;
        }
    }
}