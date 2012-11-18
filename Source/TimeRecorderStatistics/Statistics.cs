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

    using UsageStats;

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
        public Statistics(DateTime date)
        {
            this.DateTime = date;
            this.TimeAndCategories = new Dictionary<int, string>();
        }

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

        public string TotalTime
        {
            get
            {
                var minutes = this.TimeAndCategories.Count;
                if (minutes == 0)
                {
                    return string.Empty;
                }

                var h = minutes / 60;
                var m = minutes % 60;
                return string.Format("{0}:{1:00}", h, m);
            }
        }

        public ImageSource Image
        {
            get
            {
                return this.Render();
            }
        }

        public Dictionary<int, string> TimeAndCategories { get; set; }

        public void Add(string machine, IEnumerable<string> categories)
        {
            var folder = Path.Combine(TimeRecorder.RootFolder, machine);
            var path = Path.Combine(folder, TimeRecorder.FormatFileName(this.DateTime));
            if (File.Exists(path))
            {
                var lines = File.ReadAllLines(path);
                foreach (var line in lines)
                {
                    var items = line.Split(';');
                    var c = items[1];
                    bool ok = false;
                    foreach (var cat in categories)
                    {
                        if (c.IndexOf(cat) >= 0)
                        {
                            ok = true;
                        }

                        if (string.IsNullOrWhiteSpace(c) && cat == "Unknown")
                        {
                            ok = true;
                        }
                    }

                    if (!ok)
                    {
                        continue;
                    }

                    var hour = int.Parse(items[0].Substring(0, 2));
                    var min = int.Parse(items[0].Substring(3));
                    int x = hour * 60 + min;
                    this.TimeAndCategories[x] = items[1];
                }
            }
        }

        public ImageSource Render()
        {
            var r = new Rect(0, 0, 60 * 24, 80);
            var drawingVisual = new DrawingVisual();
            using (var dc = drawingVisual.RenderOpen())
            {
                dc.DrawRectangle(Brushes.Black, null, r);

                foreach (var kvp in this.TimeAndCategories)
                {
                    dc.DrawRectangle(Brushes.Green, null, new Rect(kvp.Key, 0, 1, r.Height));
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
    }
}