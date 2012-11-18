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
    }
}