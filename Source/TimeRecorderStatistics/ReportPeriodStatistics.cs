namespace TimeRecorderStatistics
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents time statistics for a report period.
    /// </summary>
    public class ReportPeriodStatistics
    {
        /// <summary>
        /// The days with activity in this period.
        /// </summary>
        private readonly HashSet<string> days = new HashSet<string>();

        public ReportPeriodStatistics()
        {
            this.CategoryTime = new Dictionary<string, int>();
        }

        /// <summary>
        /// The name of the period.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The total time (minutes) in this period.
        /// </summary>
        public int TotalTime { get; private set; }
        
        /// <summary>
        /// The total time (minutes) per category in this period.
        /// </summary>
        public Dictionary<string, int> CategoryTime { get; private set; }

        /// <summary>
        /// The total number of days with activity in this period.
        /// </summary>
        public int TotalDays
        {
            get
            {
                return this.days.Count;
            }
        }

        public void Add(DateTime time, string category)
        {
            this.TotalTime++;
            var current = 0;
            this.CategoryTime.TryGetValue(category, out current);
            this.CategoryTime[category] = current + 1;

            var day = time.ToString("yyyy-MM-dd");
            this.days.Add(day);
        }
    }
}