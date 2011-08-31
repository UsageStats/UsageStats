using System;

namespace UsageStats
{
    /// <summary>
    /// Accumulate active time.
    /// If RelativeTo is set, a % is also reported.
    /// </summary>
    public class ActiveTime
    {
        public ActiveTime()
        {
            LastActivity = DateTime.Now;
            TimeActive = TimeSpan.Zero;
        }

        public ActiveTime(ActiveTime relativeTo)
            : this()
        {
            RelativeTo = relativeTo;
        }

        public DateTime LastActivity { get; set; }
        public TimeSpan TimeActive { get; set; }
        public ActiveTime RelativeTo { get; set; }

        public double TotalSeconds
        {
            get { return TimeActive.TotalSeconds; }
        }

        public void Add(double seconds)
        {
            TimeActive = TimeActive.Add(TimeSpan.FromSeconds(seconds));
        }
        
        public bool IsNewDay()
        {
            return DateTime.Now.Day != LastActivity.Day;
        }

        public double Update(double maximumBreak)
        {
            lock (this)
            {
                var now = DateTime.Now;
                var ts = now - LastActivity;
                double sec = ts.TotalSeconds;
                if (sec < maximumBreak)
                {
                    Add(ts.TotalSeconds);
                }

                LastActivity = now;
                return sec;
            }
        }

        public override string ToString()
        {
            string s = TimeActive.ToShortString();
            if (RelativeTo != null && RelativeTo.TimeActive.TotalSeconds > 0)
            {
                double p = TimeActive.TotalSeconds/RelativeTo.TimeActive.TotalSeconds*100;
                s += String.Format(" {0:0}%", p);
            }
            return s;
        }
    }
}