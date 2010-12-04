using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UsageStats
{
    public class Histogram
    {
        private int count;
        private readonly Dictionary<int, int> data;

        private double sum;

        public Histogram()
        {
            data = new Dictionary<int, int>();
            IntervalWidth = 1;
        }

        public Histogram(double intervalWidth)
            : this()
        {
            IntervalWidth = intervalWidth;
        }

        public double IntervalWidth { get; set; }
        public double IntervalOrigin { get; set; }

        public double Average
        {
            get { return count != 0 ? sum / count : 0; }
        }

        public IEnumerable<KeyValuePair<double, int>> Data
        {
            get { return data.OrderBy(kvp => kvp.Key).Select(d => new KeyValuePair<double, int>(ToValue(d.Key), d.Value)); }
        }

        public int ToIndex(double value)
        {
            return (int)(Math.Floor((value - IntervalOrigin) / IntervalWidth + 0.5));
        }

        public double ToValue(int index)
        {
            return IntervalOrigin + IntervalWidth * index;
        }

        public void Add(double value)
        {
            sum += value;
            count++;

            int index = ToIndex(value);
            if (data.ContainsKey(index))
                data[index] = data[index] + 1;
            else
                data[index] = 1;
        }

        public string Report()
        {
            var sb = new StringBuilder();
            foreach (var d in Data)
            {
                sb.AppendFormat("  {0}: {1}\n", d.Key, d.Value);
            }
            sb.AppendFormat("  Average: {0:0.0}\n", Average);
            return sb.ToString();
        }
    }
}