using System;
using System.Text;
using System.Windows;
using System.Windows.Input;
using UsageStats.Properties;

namespace UsageStats
{
    public class Statistics : Observable
    {
        public Statistics(ActiveTime reference)
        {
            Activity = new ActiveTime(reference);
            ActivityPerHour = new TimePerHour();
            KeyboardStatistics = new KeyboardStatistics(Activity, ActivityPerHour);
            MouseStatistics = new MouseStatistics(Activity, ActivityPerHour, 1);
            InterruptionsPerCountPerHour = new CountPerHour();
        }

        public static double InactivityThreshold
        {
            get { return Settings.Default.InactivityThreshold; }
        }

        public static double InterruptionThreshold
        {
            get { return Settings.Default.InterruptionThreshold; }
        }

        public ActiveTime Activity { get; set; }

        public KeyboardStatistics KeyboardStatistics { get; set; }
        public MouseStatistics MouseStatistics { get; set; }

        public CountPerHour InterruptionsPerCountPerHour { get; set; }
        public TimePerHour ActivityPerHour { get; set; }

        public double MouseKeyboardRatio
        {
            get
            {
                double m = MouseStatistics.MouseActivity.TimeActive.TotalSeconds;
                double k = KeyboardStatistics.KeyboardActivity.TimeActive.TotalSeconds;
                return k == 0 ? 0 : m / k;
            }
        }

        public override string ToString()
        {
            return ShortReport();
        }

        public string ShortReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine(String.Format("Active time:                  {0:00}:{1:00}:{2:00}", Activity.TimeActive.TotalHours,
                                        Activity.TimeActive.Minutes, Activity.TimeActive.Seconds));
            sb.AppendLine();

            sb.AppendFormat("ACTIVITY PER HOUR ({0:0}s threshold)", Settings.Default.InactivityThreshold);
            sb.AppendLine();
            sb.AppendLine(ActivityPerHour.Report(false));
            sb.AppendLine();

            sb.AppendFormat("INTERRUPTIONS PER HOUR ({0:0}s threshold)", Settings.Default.InterruptionThreshold);
            sb.AppendLine();
            sb.Append(InterruptionsPerCountPerHour.Report(false));
            sb.AppendLine();

            if (MouseKeyboardRatio > 0)
            {
                sb.AppendLine();
                sb.AppendLine(String.Format("Mouse/Keyboard ratio: {0:0.0}", MouseKeyboardRatio));
            }

            return sb.ToString();
        }

        public string FullReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine(ShortReport());
            sb.AppendLine();

            sb.AppendLine("KEYBOARD");
            sb.Append(KeyboardStatistics.ToString());
            sb.AppendLine();

            sb.AppendLine("MOUSE");
            sb.Append(MouseStatistics.ToString());
            sb.AppendLine();

            return sb.ToString();
        }

        private void RegisterActivity()
        {
            double secondsSinceLastCheck = Activity.Update(InactivityThreshold);
            if (secondsSinceLastCheck > InterruptionThreshold)
                InterruptionsPerCountPerHour.Add(1);
            if (secondsSinceLastCheck < InactivityThreshold)
            {
                ActivityPerHour.Add(TimeSpan.FromSeconds(secondsSinceLastCheck));
            }
            RaisePropertyChanged("Activity");
        }

        public void KeyDown(string key)
        {
            KeyboardStatistics.KeyDown(key);
            RegisterActivity();
        }

        public void MouseWheel()
        {
            MouseStatistics.MouseWheel();
            RegisterActivity();
        }

        public void MouseDblClk()
        {
            MouseStatistics.MouseDblClk();
            RegisterActivity();
        }

        public void MouseDown(MouseButton mb)
        {
            MouseStatistics.MouseDown(mb);
            RegisterActivity();
        }

        public void MouseMove(Point pt)
        {
            MouseStatistics.MouseMove(pt);
            RegisterActivity();
        }

        public void MouseUp(MouseButton mb)
        {
            MouseStatistics.MouseUp(mb);
        }
    }
}