using System;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Input;
using UsageStats.Properties;

namespace UsageStats
{
    public class Statistics : Observable
    {
        [DataContract]
        public class GenericStats
        {
            [DataMember]
            public ActiveTime Activity { get; set; }
            [DataMember]
            public CountPerHour WindowSwitchesPerHour { get; set; }
            [DataMember]
            public CountPerHour InterruptionsPerHour { get; set; }
            [DataMember]
            public TimePerHour ActivityPerHour { get; set; }
        }

        public KeyboardStatistics KeyboardStatistics { get; set; }
        public MouseStatistics MouseStatistics { get; set; }
        public GenericStats Stats { get; set; }

        public Statistics(ActiveTime reference)
        {
            Stats = new GenericStats();
            Stats.Activity = new ActiveTime(reference);
            Stats.ActivityPerHour = new TimePerHour();
            KeyboardStatistics = new KeyboardStatistics(Stats.Activity, Stats.ActivityPerHour);
            MouseStatistics = new MouseStatistics(Stats.Activity, Stats.ActivityPerHour, SystemParameters.VirtualScreenWidth / SystemParameters.VirtualScreenHeight);
            Stats.InterruptionsPerHour = new CountPerHour();
            Stats.WindowSwitchesPerHour = new CountPerHour();
        }
        public int WindowSwitches { get; set; }

        public static double InactivityThreshold
        {
            get { return Settings.Default.InactivityThreshold; }
        }

        public static double InterruptionThreshold
        {
            get { return Settings.Default.InterruptionThreshold; }
        }

      
        public double MouseKeyboardRatio
        {
            get
            {
                double m = MouseStatistics.Stats.MouseActivity.TimeActive.TotalSeconds;
                double k = KeyboardStatistics.Stats.KeyboardActivity.TimeActive.TotalSeconds;
                return k == 0 ? 0 : m/k;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(String.Format("Active time:                  {0}", Stats.Activity.TimeActive.ToShortString()));
            if (MouseKeyboardRatio > 0)
                sb.AppendLine(String.Format("Mouse/Keyboard ratio: {0:0.0}", MouseKeyboardRatio));
            sb.AppendLine();

            sb.AppendLine("ACTIVITY PER HOUR");
            sb.AppendLine(Stats.ActivityPerHour.Report(false));
            sb.AppendLine();

            sb.AppendLine("INTERRUPTIONS PER HOUR");
            sb.Append(Stats.InterruptionsPerHour.Report(false));
            sb.AppendLine();

            sb.AppendLine("WINDOW SWITCHES PER HOUR");
            sb.Append(Stats.WindowSwitchesPerHour.Report(false));
            sb.AppendLine(String.Format("  Total: {0}", WindowSwitches));
            sb.AppendLine();

            return sb.ToString();
        }

        public string Report()
        {
            var sb = new StringBuilder();
            sb.AppendLine(ToString());
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
            bool isNewDay = Stats.Activity.IsNewDay();
            double secondsSinceLastCheck = Stats.Activity.Update(InactivityThreshold);
            if (secondsSinceLastCheck > InterruptionThreshold && !isNewDay)
                Stats.InterruptionsPerHour.Add(1);
            if (secondsSinceLastCheck < InactivityThreshold)
            {
                Stats.ActivityPerHour.Add(TimeSpan.FromSeconds(secondsSinceLastCheck));
            }
            RaisePropertyChanged("Activity");
        }

        public void RegisterWindowSwitch()
        {
            WindowSwitches++;
            RaisePropertyChanged("WindowSwitches");
            Stats.WindowSwitchesPerHour.Add(1);
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