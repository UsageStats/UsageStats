namespace TimeRecorderStatistics
{
    using System;
    using System.Globalization;

    public static class DateTimeExtensions
    {
        public static DateTime FirstDayOfWeek(this DateTime date, CultureInfo cultureInfo)
        {
            var firstDay = cultureInfo.DateTimeFormat.FirstDayOfWeek;
            var firstDayInWeek = date.Date;
            while (firstDayInWeek.DayOfWeek != firstDay)
            {
                firstDayInWeek = firstDayInWeek.AddDays(-1);
            }

            return firstDayInWeek;
        }

        public static int GetWeekOfYear(this DateTime time)
        {
            return cal.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        // http://blogs.msdn.com/b/shawnste/archive/2006/01/24/iso-8601-week-of-year-format-in-microsoft-net.aspx
        // This presumes that weeks start with Monday.
        // Week 1 is the 1st week of the year with a Thursday in it.
        public static int GetIso8601WeekOfYear(this DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            var day = cal.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return cal.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        // Need a calendar.  Culture's irrelevent since we specify start day of week
        private static Calendar cal = CultureInfo.InvariantCulture.Calendar;
    }
}