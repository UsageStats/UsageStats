using System;
using System.Text;

namespace UsageStats
{
    /// <summary>
    /// Count occurences and report per hour of day.
    /// </summary>
    public class CountPerHour
    {
        public int[] Count { get; private set; }

        public TimePerHour Reference { get; private set; }

        public CountPerHour(TimePerHour reference)
        {
            Count = new int[24];
            Reference = reference;
        }

        public void Increase()
        {
            Add(1);
        }


        public void Add(int count)
        {
            int hour = DateTime.Now.Hour;
            Add(hour, count);
        }

        public void Add(int hour, int count)
        {
            Count[hour] += count;
        }

        public string Report(bool showAll)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < 24; i++)
            {
                if (showAll || Count[i] > 0)
                {
                    if (Reference != null && Reference.PerHour[i].TotalSeconds > 0)
                    {
                        double ts = Reference.PerHour[i].TotalMinutes;
                        sb.AppendLine(String.Format("  {0:00}:    {1}     {2:0.0}/min", i, Count[i],Count[i]/ts));
                    } else
                    sb.AppendLine(String.Format("  {0:00}:    {1}", i, Count[i]));
                }
            }
            return sb.ToString();
        }
    }
}