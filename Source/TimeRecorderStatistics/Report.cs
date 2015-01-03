namespace TimeRecorderStatistics
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class Report
    {
        public Report(IList<string> categories, double workHourStart, double workHourEnd)
        {
            this.Categories = categories;
            this.WorkHourStart = workHourStart;
            this.WorkHourEnd = workHourEnd;
            this.TotalTime = new ReportPeriodStatistics { Name = "Total" };
            this.TimeByYear = new Dictionary<string, ReportPeriodStatistics>();
            this.TimeByMonth = new Dictionary<string, ReportPeriodStatistics>();
            this.TimeByWeek = new Dictionary<string, ReportPeriodStatistics>();
            this.TotalTimeInWorkHours = new ReportPeriodStatistics { Name = "Total" };
            this.TimeByYearInWorkHours = new Dictionary<string, ReportPeriodStatistics>();
            this.TimeByMonthInWorkHours = new Dictionary<string, ReportPeriodStatistics>();
            this.TimeByWeekInWorkHours = new Dictionary<string, ReportPeriodStatistics>();
            this.ByCategory = new Dictionary<string, List<string>>();
        }

        public IList<string> Categories { get; private set; }

        public double WorkHourStart { get; private set; }
        public double WorkHourEnd { get; private set; }

        public ReportPeriodStatistics TotalTime { get; private set; }
        public ReportPeriodStatistics TotalTimeInWorkHours { get; private set; }

        public Dictionary<string, ReportPeriodStatistics> TimeByYear { get; private set; }
        public Dictionary<string, ReportPeriodStatistics> TimeByYearInWorkHours { get; private set; }

        public Dictionary<string, ReportPeriodStatistics> TimeByMonth { get; private set; }
        public Dictionary<string, ReportPeriodStatistics> TimeByMonthInWorkHours { get; private set; }

        public Dictionary<string, ReportPeriodStatistics> TimeByWeek { get; private set; }
        public Dictionary<string, ReportPeriodStatistics> TimeByWeekInWorkHours { get; private set; }

        public Dictionary<string, List<string>> ByCategory { get; private set; }

        /// <summary>
        /// Add all logs in subfolders under the specified root folder.
        /// </summary>
        /// <param name="rootFolder">The log root folder.</param>
        public void AddAll(string rootFolder)
        {
            foreach (var machineFolder in Directory.GetDirectories(rootFolder))
            {
                this.AddFolder(machineFolder);
            }
        }

        /// <summary>
        /// Add all logs in the specified folder.
        /// </summary>
        /// <param name="machineFolder">The folder to search.</param>
        public void AddFolder(string machineFolder)
        {
            foreach (var file in Directory.GetFiles(machineFolder, "*.txt"))
            {
                this.Add(file);
            }
        }

        /// <summary>
        /// Exports the reports to the specified file name.
        /// </summary>
        /// <param name="filename">The file name (extensions will be added/changed).</param>
        public void Export(string filename)
        {
            this.TimeByYear.Add(this.TotalTime.Name, this.TotalTime);
            this.TimeByMonth.Add(this.TotalTime.Name, this.TotalTime);
            this.TimeByWeek.Add(this.TotalTime.Name, this.TotalTime);

            this.TimeByYearInWorkHours.Add(this.TotalTimeInWorkHours.Name, this.TotalTimeInWorkHours);
            this.TimeByMonthInWorkHours.Add(this.TotalTimeInWorkHours.Name, this.TotalTimeInWorkHours);
            this.TimeByWeekInWorkHours.Add(this.TotalTimeInWorkHours.Name, this.TotalTimeInWorkHours);

            this.Export(this.TimeByYear.Values, Path.ChangeExtension(filename, ".ByYear.csv"));
            this.Export(this.TimeByYearInWorkHours.Values, Path.ChangeExtension(filename, ".ByYearInWorkHours.csv"));
            this.Export(this.TimeByMonth.Values, Path.ChangeExtension(filename, ".ByMonth.csv"));
            this.Export(this.TimeByMonthInWorkHours.Values, Path.ChangeExtension(filename, ".ByMonthInWorkHours.csv"));
            this.Export(this.TimeByWeek.Values, Path.ChangeExtension(filename, ".ByWeek.csv"));
            this.Export(this.TimeByWeekInWorkHours.Values, Path.ChangeExtension(filename, ".ByWeekInWorkHours.csv"));

            foreach (var c in this.ByCategory.Keys)
            {
                File.WriteAllLines(Path.ChangeExtension(filename, "." + c + ".txt"), this.ByCategory[c].OrderBy(x => x));
            }
        }

        private void Export(IEnumerable<ReportPeriodStatistics> values, string path)
        {
            var periods = values.ToArray();
            var categories = new HashSet<string>(periods.SelectMany(v => v.CategoryTime.Keys)).OrderBy(c => c).ToArray();

            using (var w = new StreamWriter(path))
            {
                w.Write("Period");
                foreach (var c in categories)
                    w.Write(";" + c);
                w.Write(";Total;Days");
                w.WriteLine();
                foreach (var v in periods.OrderBy(p => p.Name))
                {
                    w.Write("{0}", v.Name);
                    foreach (var c in categories)
                    {
                        int time = 0;
                        v.CategoryTime.TryGetValue(c, out time);
                        w.Write(";{0:0.00}", time / 60d);
                    }

                    w.Write(";{0:0.00};{1}", v.TotalTime / 60d, v.TotalDays);
                    w.WriteLine();
                }
            }
        }

        public void Add(string filename)
        {
            DateTime date;
            if (!DateTime.TryParse(Path.GetFileNameWithoutExtension(filename), out date))
            {
                return;
            }

            foreach (var line in File.ReadAllLines(filename))
            {
                this.Add(date, line);
            }
        }

        public void Add(DateTime date, string line)
        {
            var trimmedLine = line.Trim().Trim('\0');
            var items = trimmedLine.Split(';');
            if (items.Length != 2)
            {
                return;
            }

            var week = string.Format("{0}-{1:00}", date.Year, date.GetWeekOfYear());
            var year = string.Format("{0}", date.Year);
            var month = string.Format("{0}-{1:00}", date.Year, date.Month);

            var hour = int.Parse(items[0].Substring(0, 2));
            var min = int.Parse(items[0].Substring(3));
            int minutes = (hour * 60) + min;
            double hours = hour + (min / 60.0);
            var inWorkHours = hours >= this.WorkHourStart && hours <= this.WorkHourEnd;

            var time = new DateTime(date.Year, date.Month, date.Day, hour, min, 0);

            Action<string> addToCategory = category =>
                {
                    this.TotalTime.Add(time, category);
                    this.Add(this.TimeByYear, year, time, category);
                    this.Add(this.TimeByMonth, month, time, category);
                    this.Add(this.TimeByWeek, week, time, category);
                    if (inWorkHours)
                    {
                        this.TotalTimeInWorkHours.Add(time, category);
                        this.Add(this.TimeByYearInWorkHours, year, time, category);
                        this.Add(this.TimeByMonthInWorkHours, month, time, category);
                        this.Add(this.TimeByWeekInWorkHours, week, time, category);
                    }

                    this.ByCategory.GetOrCreate(category, () => new List<string>()).Add(string.Format("{0}-{1:00}-{2:00} {3}", date.Year, date.Month, date.Day, line));
                };

            bool notFound = true;
            foreach (var category in this.Categories)
            {
                if (line.IndexOf(category, StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    addToCategory(category);
                    notFound = false;
                }
            }

            if (notFound)
            {
                addToCategory("Unknown");
            }
        }

        private void Add(Dictionary<string, ReportPeriodStatistics> d, string key, DateTime time, string category)
        {
            d.GetOrCreate(key, () => new ReportPeriodStatistics { Name = key }).Add(time, category);
        }
    }
}