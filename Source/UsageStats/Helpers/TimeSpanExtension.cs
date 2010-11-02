using System;

namespace UsageStats
{
    public static class TimeSpanExtension
    {
        public static string ToShortString(this TimeSpan span)
        {
            return String.Format("{0:00}:{1:00}:{2:00}", span.Hours, span.Minutes, span.Seconds);
        }
    }
}
