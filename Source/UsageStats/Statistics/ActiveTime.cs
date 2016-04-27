using System;
using System.Runtime.Serialization;
namespace UsageStats
{
    /// <summary>
    /// Accumulate active time.
    /// If RelativeTo is set, a % is also reported.
    /// </summary>
    /// 
    [DataContract]
    public class ActiveTime
    {
        public ActiveTime()
        {
            FirstActivity = DateTime.Now;
            LastActivity = DateTime.Now;
            TimeActive = TimeSpan.Zero;
        }

        public ActiveTime(ActiveTime relativeTo)
            : this()
        {
            RelativeTo = relativeTo;
        }

        [IgnoreDataMember]
        public DateTime LastActivity { get; set; }
        [IgnoreDataMember]
        public DateTime FirstActivity { get; set; }
        
        [IgnoreDataMember]
        public TimeSpan TimeActive { get; set; }
        [IgnoreDataMember]
        public ActiveTime RelativeTo { get; set; }

        [DataMember]
        public long LastActivityTimeStamp
        {
            get
            {
                return Helpers.DateTimeHelper.ToUnixTime(LastActivity);
            }
            set
            {
                LastActivity = Helpers.DateTimeHelper.FromUnixTime(value);
            }
        }

        [DataMember]
        public long FirstActivityTimeStamp
        {
            get
            {
                return Helpers.DateTimeHelper.ToUnixTime(FirstActivity);
            }
            set
            {
                FirstActivity = Helpers.DateTimeHelper.FromUnixTime(value);
            }
        }



        [DataMember]
        public double SecondsActive
        {
            get
            {
                return TimeActive.TotalSeconds;
            }
            set
            {
                TimeActive.Add(new TimeSpan(0, 0, (int) value));
            }
        }


        [IgnoreDataMember]
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