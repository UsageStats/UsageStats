using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace UsageStats
{
    /// <summary>
    /// Count occurences and report per hour of day.
    /// </summary>
    public class CountPerHour : IEnumerable<KeyValuePair<int,int>>
    {
        public CountPerHour(TimePerHour reference = null)
        {
            Count = new int[24];
            Reference = reference;
        }

        public int[] Count { get; private set; }

        public TimePerHour Reference { get; private set; }

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
                        sb.Append(String.Format("  {0:00}:    {1}", i, Count[i]));
                        sb.AppendLine(String.Format("     {0:0.0}/min", Count[i]/ts));
                    }
                    else
                        sb.AppendLine(String.Format("  {0:00}:    {1}", i, Count[i]));
                }
            }
            return sb.ToString();
        }

        public IEnumerator<KeyValuePair<int, int>> GetEnumerator()
        {
            for (int i=0;i<Count.Length;i++)
                yield return new KeyValuePair<int, int>(i,Count[i]);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}