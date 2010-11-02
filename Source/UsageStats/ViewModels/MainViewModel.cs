using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace UsageStats
{
    public class MainViewModel : Observable
    {
        private const string DefaultApplicationList =
            @"Microsoft Visual Studio
Reflector

Microsoft Outlook
Gmail

Word
Excel
Powerpoint

Adobe

Microsoft Internet Explorer
Google Chrome
Wikipedia

Windows Explorer";

        private readonly InterceptKeys keys;
        private readonly InterceptMouse mouse;
        private readonly DispatcherTimer timer;
        private readonly Stopwatch watch;
        private Point lastPoint;

        public bool AlwaysOnTop { get { return Settings.AlwaysOnTop; } }

        public MainViewModel()
        {
            Settings = new SettingsViewModel();
            ScreenResolution = 96;

            if (String.IsNullOrEmpty(Settings.ReportPath))
            {
                Settings.ReportPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                                                   "UsageStats");
            }
            if (String.IsNullOrEmpty(Settings.ApplicationList))
            {
                Settings.ApplicationList = DefaultApplicationList;
            }

            InitStatistics();

            watch = new Stopwatch();
            watch.Start();

            timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            timer.Tick += TimerTick;
            timer.Start();

            Hook.CreateKeyboardHook(KeyReader);
            keys = new InterceptKeys();
            mouse = new InterceptMouse(MouseHandler);

        }

        public DateTime RecordingStarted { get; set; }
        public DateTime FirstActivity { get; set; }
        public DateTime LastActivity { get; set; }

        public ActiveTime Recording { get; set; }

        public SettingsViewModel Settings { get; private set; }

        public string Title
        {
            get { return "Application Usage Statistics"; }
        }

        public double ScreenResolution { get; set; }

        public string CurrentApplication { get; set; }
        public Point CurrentPosition { get; set; }
        public Statistics CurrentStatistics { get; set; }
        public Statistics Statistics { get; set; }
        public Dictionary<string, Statistics> ApplicationUsage { get; set; }

        public Statistics GeneralStatistics
        {
            get { return CurrentStatistics; }
        }

        public MouseStatistics MouseStatistics
        {
            get { return CurrentStatistics.MouseStatistics; }
        }

        public KeyboardStatistics KeyboardStatistics
        {
            get { return CurrentStatistics.KeyboardStatistics; }
        }

        public string ApplicationReport
        {
            get { return CreateApplicationReport(); }
        }
      
        public string Report
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendLine(String.Format("Recording started: {0}", RecordingStarted));
                sb.AppendLine(String.Format("Recording ended:   {0}", Recording.LastCheck));
                sb.AppendLine(String.Format("First activity:    {0}", FirstActivity));
                sb.AppendLine(String.Format("Last activity:     {0}", LastActivity));
                TimeSpan duration = LastActivity - FirstActivity;
                sb.AppendLine(String.Format("Duration:                     {0}", duration.ToShortString()));
                sb.AppendLine();
                sb.AppendLine(Statistics.ToString());
                return sb.ToString();
            }
        }

        private void InitStatistics()
        {
            FirstActivity = new DateTime(0);

            RecordingStarted = DateTime.Now;
            Recording = new ActiveTime();
            Statistics = new Statistics(Recording);
            CurrentStatistics = Statistics;

            ApplicationUsage = new Dictionary<string, Statistics>();

            AddApplications();

            lastPoint = WindowHelper.GetCursorPos();
        }

        private string CreateApplicationReport()
        {
            if (ApplicationUsage.Count == 0)
                return null;
            var sb = new StringBuilder();
            int longest = ApplicationUsage.Keys.Max(e => e.Length);
            foreach (var kvp in ApplicationUsage)
            {
                Statistics s = kvp.Value;
                if (s.Activity.TimeActive.TotalSeconds == 0)
                {
                    continue;
                }
                sb.AppendLine(String.Format("{0} {1}", kvp.Key.PadRight(longest + 2), s.Activity));
            }
            return sb.ToString();
        }

        public void SaveReports()
        {
            string path = Settings.ReportPath;
            SaveReport(Statistics, path);
            foreach (var kvp in ApplicationUsage)
            {
                Statistics s = kvp.Value;
                SaveReport(s, Path.Combine(path, kvp.Key));
            }
        }

        public void SaveReport(Statistics s, string prefix)
        {
            if (!String.IsNullOrEmpty(prefix))
            {
                if (!Directory.Exists(prefix))
                {
                    Directory.CreateDirectory(prefix);
                }
                prefix += "/";
            }
            string path = String.Format("{0}Report_{1:yyyy-MM-dd}.txt", prefix, RecordingStarted);
            path = FindUniqueName(path);
            var w = new StreamWriter(path);
            w.Write(CreateReport(s));
            w.Close();

            if (Settings.DisplayDate)
            {
                string txt = String.Format("{0:yyyy-MM-dd}", RecordingStarted);

                s.MouseStatistics.ClickMap.DrawDate(txt);
                s.MouseStatistics.DoubleClickMap.DrawDate(txt);
                s.MouseStatistics.TraceMap.DrawDate(txt);
                s.MouseStatistics.DragTraceMap.DrawDate(txt);
            }

            string imgPath = String.Format("{0}ClickMap_{1:yyyy-MM-dd}.png", prefix, RecordingStarted);
            SaveBitmap(s.MouseStatistics.ClickMap.Source, FindUniqueName(imgPath));

            imgPath = String.Format("{0}DoubleClickMap_{1:yyyy-MM-dd}.png", prefix, RecordingStarted);
            SaveBitmap(s.MouseStatistics.DoubleClickMap.Source, FindUniqueName(imgPath));

            imgPath = String.Format("{0}TraceMap_{1:yyyy-MM-dd}.png", prefix, RecordingStarted);
            SaveBitmap(s.MouseStatistics.TraceMap.Source, FindUniqueName(imgPath));

            imgPath = String.Format("{0}DragTraceMap_{1:yyyy-MM-dd}.png", prefix, RecordingStarted);
            SaveBitmap(s.MouseStatistics.DragTraceMap.Source, FindUniqueName(imgPath));

            string totalsPath = String.Format("{0}Totals.csv", prefix);
            AppendTotals(s, totalsPath);
        }

        private void AppendTotals(Statistics s, string path)
        {
            StreamWriter w;
            if (File.Exists(path))
            {
                w = new StreamWriter(path, true);
            }
            else
            {
                w = new StreamWriter(path);
                w.WriteLine("Date;FirstActivity;LastActivity;ActiveTime;MouseKeyboardRatio;Keystrokes;KeystrokesPerMinute;LeftClicks;MiddleClicks;RightClicks;DoubleClicks;MouseDistance;WheelDistance;ClicksPerMinute;");
            }
            w.Write("{0:yyyy-MM-dd};", RecordingStarted);
            w.Write("{0:HH:mm:ss};", FirstActivity);
            w.Write("{0:HH:mm:ss};", LastActivity);
            w.Write("{0};", s.Activity.TimeActive.ToShortString());
            w.Write("{0:0.0};", s.MouseKeyboardRatio);
            w.Write("{0};", s.KeyboardStatistics.KeyStrokes);
            w.Write("{0:0};", s.KeyboardStatistics.KeyStrokesPerMinute);
            w.Write("{0:0};", s.MouseStatistics.LeftMouseClicks);
            w.Write("{0:0};", s.MouseStatistics.MiddleMouseClicks);
            w.Write("{0:0};", s.MouseStatistics.RightMouseClicks);
            w.Write("{0:0};", s.MouseStatistics.MouseDoubleClicks);
            w.Write("{0:0.0};", s.MouseStatistics.MouseDistance);
            w.Write("{0:0};", s.MouseStatistics.MouseWheelDistance);
            w.Write("{0:0};", s.MouseStatistics.MouseClicksPerMinute);
            w.WriteLine();

            w.Close();
        }

        private static void SaveBitmap(BitmapSource bmp, string fileName)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            using (FileStream stm = File.Create(fileName))
            {
                encoder.Save(stm);
            }
        }

        private string CreateReport(Statistics s)
        {
            var sb = new StringBuilder();
            if (s == Statistics)
            {
                sb.Append(Report);
                sb.AppendLine();
                sb.Append(CreateApplicationReport());
                sb.AppendLine();
            }

            sb.Append(s.Report());
            return sb.ToString();
        }

        private static string FindUniqueName(string path)
        {
            string newPath = path;
            string dir = Path.GetDirectoryName(path);
            string name = Path.GetFileNameWithoutExtension(path);
            string ext = Path.GetExtension(path);
            int i = 1;
            while (File.Exists(newPath))
            {
                string newName = name + String.Format("_{0:00}", i++);
                newPath = Path.Combine(dir, newName + ext);
            }
            return newPath;
        }


        public void Dispose()
        {
            if (mouse != null)
            {
                mouse.Dispose();
            }
            if (keys != null)
            {
                keys.Dispose();
            }
        }

        public void AddApplications(TextReader r)
        {
            while (true)
            {
                string app = r.ReadLine();
                if (app == null)
                    break;
                app = app.Trim();

                if (String.IsNullOrWhiteSpace(app))
                {
                    continue;
                }
                AddApplication(app);
            }
        }

        public void AddApplications()
        {
            using (var s = new StringReader(Settings.ApplicationList))
            {
                AddApplications(s);
            }
        }


        private void AddApplication(string appName)
        {
            if (!ApplicationUsage.ContainsKey(appName))
                ApplicationUsage.Add(appName, new Statistics(Statistics.Activity));
        }

        public IEnumerable<Statistics> GetCurrentStatistics()
        {
            if (CurrentApplication != null)
            {
                foreach (string app in ApplicationUsage.Keys)
                {
                    if (CurrentApplication.Contains(app))
                    {
                        yield return ApplicationUsage[app];
                    }
                }
            }
        }

        private void MouseHandler(IntPtr wparam, IntPtr lparam)
        {
            switch ((MouseMessages)wparam)
            {
                case MouseMessages.WM_LBUTTONDOWN:
                    MouseDown(MouseButton.Left);
                    break;
                case MouseMessages.WM_LBUTTONUP:
                    MouseUp(MouseButton.Left);
                    break;
                case MouseMessages.WM_MBUTTONDOWN:
                    MouseDown(MouseButton.Middle);
                    break;
                case MouseMessages.WM_RBUTTONDOWN:
                    MouseDown(MouseButton.Right);
                    break;
                case MouseMessages.WM_LBUTTONDBLCLK:
                case MouseMessages.WM_MBUTTONDBLCLK:
                case MouseMessages.WM_RBUTTONDBLCLK:
                    MouseDblClk();
                    break;
                case MouseMessages.WM_MOUSEWHEEL:
                    // wparam: The high-order word indicates the distance the wheel is rotated, expressed in multiples or divisions of 
                    // WHEEL_DELTA, which is 120. A positive value indicates that the wheel was rotated forward, away from the user; 
                    // a negative value indicates that the wheel was rotated backward, toward the user.
                    MouseWheel();
                    break;
            }
        }

        private void MouseUp(MouseButton mouseButton)
        {
            Statistics.MouseUp(mouseButton);
            foreach (Statistics s in GetCurrentStatistics())
            {
                s.MouseUp(mouseButton);
            }
        }

        private void MouseDown(MouseButton mouseButton)
        {
            Statistics.MouseDown(mouseButton);
            foreach (Statistics s in GetCurrentStatistics())
            {
                s.MouseDown(mouseButton);
            }
            UpdateFirstAndLast();
        }

        private void MouseDblClk()
        {
            Statistics.MouseDblClk();
            foreach (Statistics s in GetCurrentStatistics())
            {
                s.MouseDblClk();
            }
        }

        private void MouseWheel()
        {
            Statistics.MouseWheel();
            foreach (Statistics s in GetCurrentStatistics())
            {
                s.MouseWheel();
            }
            UpdateFirstAndLast();
        }

        public void KeyReader(IntPtr wParam, IntPtr lParam)
        {
            int virtualKey = Marshal.ReadInt32(lParam);
            Key k = KeyInterop.KeyFromVirtualKey(virtualKey);
            var kc = new KeyConverter();
            string keyName = kc.ConvertToString(k);

            KeyDown(keyName);
        }

        private void KeyDown(string keyName)
        {
            Statistics.KeyDown(keyName);
            foreach (Statistics s in GetCurrentStatistics())
            {
                s.KeyDown(keyName);
            }
            UpdateFirstAndLast();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            //double deltaTime = watch.ElapsedMilliseconds * 0.001;
            //watch.Reset(); watch.Start();

            Point pt = WindowHelper.GetCursorPos();
            double dx = pt.X - lastPoint.X;
            double dy = pt.Y - lastPoint.Y;
            double dl = Math.Sqrt(dx * dx + dy * dy);
            bool moved = dl > 0.3;
            if (moved)
            {
                MouseMove(pt);
            }
            lastPoint = pt;

            // Update current position
            CurrentPosition = pt;

            // Update current application (from window title)
            CurrentApplication = WindowHelper.GetForegroundWindowText();

            // Update total recording time
            Recording.Update(double.MaxValue);

            RaisePropertyChanged("CurrentApplication");
            RaisePropertyChanged("CurrentPosition");
            RaisePropertyChanged("MouseDistanceText");

            RaisePropertyChanged("MouseActivity");
            RaisePropertyChanged("Activity");

            RaisePropertyChanged("ApplicationReport");

            RaisePropertyChanged("MouseStatistics");
            RaisePropertyChanged("KeyboardStatistics");
            RaisePropertyChanged("Statistics");
            RaisePropertyChanged("Report");

            // Check if we passed midnight
            if (RecordingStarted.Day != DateTime.Now.Day)
            {
                // Save report for today and reset
                SaveReports();
                InitStatistics();
            }
        }


        private void MouseMove(Point pt)
        {
            Statistics.MouseMove(pt);
            foreach (Statistics s in GetCurrentStatistics())
            {
                s.MouseMove(pt);
            }
            UpdateFirstAndLast();
        }


        public void UpdateFirstAndLast()
        {
            if (FirstActivity.Ticks == 0)
            {
                FirstActivity = DateTime.Now;
            }
            LastActivity = DateTime.Now;
            RaisePropertyChanged("FirstActivity");
            RaisePropertyChanged("LastActivity");
        }

        public void OnClosed()
        {
            Settings.Save();
        }

        public void OnSettingsChanged()
        {
            Settings.Save();

            RaisePropertyChanged("AlwaysOnTop");
            AddApplications();
        }
    }
}