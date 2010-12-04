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
                return KeyUsage.OrderByDescending(kvp => kvp.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
        }

        public Dictionary<string, int> KeyCountPerHourList
        {
            get
            {
                return KeyCountPerHour.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value);
            }
        }

        public Dictionary<string, int> TypingSpeedList
        {
            get { return TypingSpeed.Data.ToDictionary(v => v.Key.ToString(), v => v.Value); }
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

        public string Report
        {
            get { return ToString(); }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(String.Format(" Keystrokes:    {0}", KeyStrokes));
            sb.AppendLine(String.Format(" Activity:      {0}", KeyboardActivity));
            sb.AppendLine();
            sb.AppendLine(String.Format(" Average speed: {0:0} keystrokes/min", TypingSpeed.Average));
            sb.AppendLine();
            //sb.AppendLine(" Typing speed (keystrokes per minute):");
            //sb.AppendLine(TypingSpeed.Report());
            //sb.AppendLine();
            sb.AppendLine(" Keystrokes per hour:");
            sb.AppendLine(KeyCountPerHour.Report(false));
            sb.AppendLine();
            var list = KeyUsage.ToList().OrderByDescending(kvp => kvp.Value);
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
            RaisePropertyChanged("TypingSpeedList");

            KeyStrokes++;
            RaisePropertyChanged("KeyStrokes");

            KeyCountPerHour.Increase();
            RaisePropertyChanged("KeyCountPerHour");
            RaisePropertyChanged("KeyCountPerHourList");
            RaisePropertyChanged("Report");
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