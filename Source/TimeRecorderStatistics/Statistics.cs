namespace TimeRecorderStatistics
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    public class Observable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string property)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(property));
            }
        }
    }

    public class Statistics : Observable
    {
        public Statistics(DateTime date, List<string> categories)
        {
            this.DateTime = date;
            this.TimeAndCategories = new Dictionary<int, string>();
            this.categories = categories;
            this.DayStartHour = 8.5;
            this.DayEndHour = 16.5;
        }

        public bool RenderPerMachine { get; set; }
        public bool RenderSelectedOnly { get; set; }

        private readonly List<string> categories;

        public DateTime DateTime { get; set; }

        public string Date
        {
            get
            {
                return this.DateTime.ToShortDateString();
            }
        }

        public string Day
        {
            get
            {
                return this.DateTime.DayOfWeek.ToString();
            }
        }

        public string TotalTimeString
        {
            get
            {
                return ToTimeString(this.TotalTime);
            }
        }

        public string SelectedTimeString
        {
            get
            {
                return ToTimeString(this.SelectedTime);
            }
        }

        public string SelectedPercentage
        {
            get
            {
                return string.Format("{0:0.0} %", 100.0 * this.SelectedTime / this.TotalTime);
            }
        }

        public string TimeInfo
        {
            get
            {
                if (this.SelectedTime == this.TotalTime || this.SelectedTime == 0) return ToTimeString(this.TotalTime);
                return string.Format("{0} / {1} ({2})", ToTimeString(this.SelectedTime), ToTimeString(this.TotalTime), this.SelectedPercentage);
            }
        }

        public string WorkHoursTimeInfo
        {
            get
            {
                if (this.SelectedWorkHoursTime == this.TotalTime || this.SelectedWorkHoursTime == 0)
                {
                    return string.Empty;
                }

                var percentage = 100.0 * this.SelectedWorkHoursTime / ((this.DayEndHour - this.DayStartHour) * 60);
                return string.Format("{0} ({1:0.0} %)", ToTimeString(this.SelectedWorkHoursTime), percentage);
            }
        }

        public static string ToTimeString(int minutes)
        {
            if (minutes == 0)
            {
                return string.Empty;
            }

            var h = minutes / 60;
            var m = minutes % 60;
            return string.Format("{0}:{1:00}", h, m);
        }

        public ImageSource Image
        {
            get
            {
                return this.Render();
            }
        }

        public Dictionary<int, string> TimeAndCategories { get; set; }

        private List<string> machines = new List<string>();

        public int TotalTime { get; private set; }

        public int SelectedTime { get; private set; }
        public int SelectedWorkHoursTime { get; private set; }

        public void Add(string machine)
        {
            this.machines.Add(machine);
            var folder = Path.Combine(TimeRecorder.TimeRecorder.RootFolder, machine);
            var path = Path.Combine(folder, TimeRecorder.TimeRecorder.FormatFileName(this.DateTime));
            if (File.Exists(path))
            {
                var lines = File.ReadAllLines(path);
                foreach (var line in lines)
                {
                    var items = line.Split(';');
                    if (items.Length != 2)
                    {
                        continue;
                    }

                    var category = items[1];
                    var hour = int.Parse(items[0].Substring(0, 2));
                    var min = int.Parse(items[0].Substring(3));
                    int minutes = (hour * 60) + min;
                    double hours = hour + (min / 60.0);

                    if (string.IsNullOrWhiteSpace(category))
                    {
                        category = "Unknown";
                    }

                    string current;
                    if (this.TimeAndCategories.TryGetValue(minutes, out current))
                    {
                        current += " ";
                    }

                    this.TimeAndCategories[minutes] = current + machine + ":" + category;

                    if (this.ValidCategory(category))
                    {
                        this.SelectedTime++;
                        if (hours >= this.DayStartHour && hours <= this.DayEndHour && this.IsWeekDay())
                        {
                            this.SelectedWorkHoursTime++;
                        }
                    }

                    this.TotalTime++;
                }
            }
        }

        private bool ValidCategory(string c)
        {
            foreach (var cat in this.categories)
            {
                if (c.IndexOf(cat) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        public ImageSource Render()
        {
            var r = new Rect(0, 0, 60 * 24, 80);
            var drawingVisual = new DrawingVisual();
            using (var dc = drawingVisual.RenderOpen())
            {
                dc.DrawRectangle(Brushes.Black, null, r);

                if (this.IsWeekDay())
                {
                    dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(60, 0, 20, 255)), null, new Rect(this.DayStartHour * 60, 0, (this.DayEndHour - this.DayStartHour) * 60, r.Height));
                }

                int n = this.machines.Count;
                foreach (var kvp in this.TimeAndCategories)
                {
                    Brush brush = Brushes.Green;
                    if (this.ValidCategory(kvp.Value))
                    {
                        brush = Brushes.DarkRed;
                    }
                    else
                    {
                        if (this.RenderSelectedOnly)
                        {
                            continue;
                        }
                    }

                    if (this.RenderPerMachine)
                    {
                        for (int i = 0; i < n; i++)
                        {
                            if (!kvp.Value.Contains(this.machines[i]))
                            {
                                continue;
                            }

                            dc.DrawRectangle(brush, null, new Rect(kvp.Key, r.Height * i / n, 1, r.Height / n));
                        }
                    }
                    else
                    {
                        dc.DrawRectangle(brush, null, new Rect(kvp.Key, 0, 1, r.Height));
                    }
                }

                var lineBrush = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255));
                for (int h = 1; h < 24; h++)
                {
                    int x = h * 60;
                    dc.DrawRectangle(lineBrush, null, new Rect(x, 0, 1, r.Height));
                }

                dc.DrawRectangle(lineBrush, null, new Rect(0, r.Height - 1, r.Width, 1));
            }

            var rtb = new RenderTargetBitmap((int)r.Width, (int)r.Height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(drawingVisual);
            return rtb;
        }

        private bool IsWeekDay()
        {
            return this.DateTime.DayOfWeek >= DayOfWeek.Monday && this.DateTime.DayOfWeek <= DayOfWeek.Friday;
        }

        protected double DayStartHour { get; set; }

        protected double DayEndHour { get; set; }

        public static ImageSource RenderHeader()
        {
            var r = new Rect(0, 0, 60 * 24, 40);
            var drawingVisual = new DrawingVisual();
            using (var dc = drawingVisual.RenderOpen())
            {
                dc.DrawRectangle(Brushes.Black, null, r);

                var lineBrush = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255));
                for (int h = 1; h < 24; h++)
                {
                    int x = h * 60;
                    var ft = new FormattedText(
                        h + ":00",
                        CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight,
                        new Typeface("Segoe UI"),
                        12,
                        Brushes.Gray);
                    ft.TextAlignment = TextAlignment.Center;
                    dc.DrawText(ft, new Point(x, 2));
                }

                dc.DrawRectangle(lineBrush, null, new Rect(0, r.Height - 1, r.Width, 1));
            }

            var rtb = new RenderTargetBitmap((int)r.Width, (int)r.Height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(drawingVisual);
            return rtb;
        }

        /// <summary>
        /// Gets the tool tip for the specified time.
        /// </summary>
        /// <param name="minutes">The minutes of the day.</param>
        /// <returns>The tooltip.</returns>
        public string GetToolTip(int minutes)
        {
            string value;
            return this.TimeAndCategories.TryGetValue(minutes, out value) ? value : null;
        }
    }
}