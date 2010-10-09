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
            LastCheck = DateTime.Now;
            TimeActive = TimeSpan.Zero;
        }

        public ActiveTime(ActiveTime relativeTo)
            : this()
        {
            RelativeTo = relativeTo;
        }

        public DateTime LastCheck { get; set; }
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

        public double Update(double maximumBreak)
        {
            lock (this)
            {
                var now = DateTime.Now;
                var ts = now - LastCheck;
                double sec = ts.TotalSeconds;
                if (sec < maximumBreak)
                {
                    Add(ts.TotalSeconds);
                }

                LastCheck = now;
                return sec;
            }
        }

        public override string ToString()
        {
            string s = String.Format("{0:00}:{1:00}:{2:00}", TimeActive.Hours, TimeActive.Minutes, TimeActive.Seconds);
            if (RelativeTo != null && RelativeTo.TimeActive.TotalSeconds > 0)
            {
                double p = TimeActive.TotalSeconds/RelativeTo.TimeActive.TotalSeconds*100;
                s += String.Format(" {0:0}%", p);
            }
            return s;
        }
    }
}