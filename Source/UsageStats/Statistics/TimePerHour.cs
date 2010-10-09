using System;
using System.Text;

namespace UsageStats
{
    /// <summary>
    /// Accumulate time and report per hour of day.
    /// </summary>
    public class TimePerHour
    {
        public TimeSpan[] PerHour { get; private set; }

        public TimePerHour()
        {
            PerHour = new TimeSpan[24];
        }


        public void Add(TimeSpan count)
        {
            int hour = DateTime.Now.Hour;
            Add(hour, count);
        }

        public void Add(int hour, TimeSpan count)
        {
            PerHour[hour] += count;
        }

        public string Report(bool showAll)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < 24; i++)
            {
                if (showAll || PerHour[i].TotalSeconds > 0)
                    sb.AppendLine(String.Format("  {0:00}: {1:00}:{2:00}:{3:00}", i, PerHour[i].Hours,PerHour[i].Minutes,PerHour[i].Seconds));
            }
            return sb.ToString();
        }
    }
}