using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UsageStats
{
    public class KeyboardStatistics : Observable
    {
        public KeyboardStatistics(ActiveTime total, TimePerHour activityPerHour)
        {
            KeyUsage = new Dictionary<string, int>();
            KeyboardActivity = new ActiveTime(total);
            KeyCountPerHour = new CountPerHour(activityPerHour);
            TypingSpeed = new Histogram(25);
        }

        public int KeyStrokes { get; set; }
        public Dictionary<string, int> KeyUsage { get; set; }


        public ActiveTime KeyboardActivity { get; set; }
        public CountPerHour KeyCountPerHour { get; set; }

        // todo: should not be neccessary to duplicate these dictionaries in order to get the BarChart ItemsControl to work?
        public Dictionary<string, int> KeyUsageList
        {
            get
            {
                var d = new Dictionary<string, int>();
                foreach (var kvp in KeyUsage.OrderByDescending(kvp => kvp.Value))
                    d.Add(kvp.Key, kvp.Value);
                return d;
            }
        }

        public Dictionary<string, int> KeyCountPerHourList
        {
            get
            {
                var d = new Dictionary<string, int>();
                foreach (var kvp in KeyCountPerHour)
                    d.Add(kvp.Key.ToString(), kvp.Value);
                return d;
            }
        }

        public Histogram TypingSpeed { get; set; }

        public double KeyStrokesPerMinute
        {
            get
            {
                double min = KeyboardActivity.TotalSeconds / 60;
                return min > 0 ? KeyStrokes / min : 0;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(String.Format(" Keystrokes:    {0}", KeyStrokes));
            sb.AppendLine(String.Format(" Activity:      {0}", KeyboardActivity));
            sb.AppendLine();
            //sb.AppendLine(" Typing speed (keystrokes per minute):");
            //sb.AppendLine(TypingSpeed.Report());
            //sb.AppendLine();
            sb.AppendLine(" Keystrokes per hour:");
            sb.AppendLine(KeyCountPerHour.Report(false));
            sb.AppendLine();
            IOrderedEnumerable<KeyValuePair<string, int>> list = KeyUsage.ToList().OrderByDescending(kvp => kvp.Value);
            if (list.Count() > 0)
            {
                int longest = list.Max(kvp => kvp.Key.ToString().Length);
                foreach (var kvp in list)
                {
                    double p = KeyStrokes > 0 ? 1.0 * kvp.Value / KeyStrokes : 0;
                    sb.AppendLine(String.Format(" {0} {1:####} {2:0.0%}", kvp.Key.PadRight(longest), kvp.Value, p));
                }
            }
            return sb.ToString();
        }

        public void KeyDown(string key)
        {
            AddKeyUsage(key);
            RaisePropertyChanged("KeyUsageList");

            double sec = KeyboardActivity.Update(Statistics.InactivityThreshold);
            RaisePropertyChanged("KeyboardActivity");

            if (sec < 4 && sec > 0.1)
            {
                double perMinute = 60 / sec;
                TypingSpeed.Add(perMinute);
            }
            RaisePropertyChanged("TypingSpeed");

            KeyStrokes++;
            RaisePropertyChanged("KeyStrokes");

            KeyCountPerHour.Increase();
            RaisePropertyChanged("KeyCountPerHour");
            RaisePropertyChanged("KeyCountPerHourList");
        }

        private void AddKeyUsage(string key)
        {
            if (KeyUsage.ContainsKey(key))
            {
                KeyUsage[key] = KeyUsage[key] + 1;
            }
            else
            {
                KeyUsage.Add(key, 1);
            }
        }
    }
}