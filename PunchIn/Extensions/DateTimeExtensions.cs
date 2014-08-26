namespace PunchIn.Extensions
{
    using System;
    using System.Globalization;

    public static class DateTimeExtensions
    {
        /// <summary>
        /// Gets an integer representing the WeekOfYear number for the specified date
        /// </summary>
        /// <param name="dt">The DateTime</param>
        /// <returns>WeekOfYear integer</returns>
        public static int GetWeekOfYear(this DateTime dt)
        {
            return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dt, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }
        /// <summary>
        /// Gets DateTime @ midnight of the WeekOfYear for the current year
        /// </summary>
        /// <param name="weekOfYear">Current WeekOfYear</param>
        /// <returns>DateTime @ midnight for WeekOfYear</returns>
        public static DateTime GetWeekOfYearDate(this int weekOfYear)
        {
            return weekOfYear.GetWeekOfYearDate(DateTime.Now.Year);
        }
        /// <summary>
        /// Gets DateTime @ midnight of the WeekOfYear for a given year
        /// </summary>
        /// <param name="weekOfYear">Current WeekOfYear</param>
        /// <param name="year">The year</param>
        /// <returns>DateTime @ midnight for WeekOfYear</returns>
        public static DateTime GetWeekOfYearDate(this int weekOfYear, int year)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            DateTime firstThursday = jan1.AddDays(daysOffset);
            int firstWeek = firstThursday.GetWeekOfYear();

            var weekNum = weekOfYear;
            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }
            var result = firstThursday.AddDays(weekNum * 7);
            return result.AddDays(-3);
        }
        /// <summary>
        /// Gets the start of week DateTime
        /// </summary>
        /// <param name="dt"></param>
        /// <returns>Monday midnight DateTime</returns>
        public static DateTime StartOfWeek(this DateTime dt)
        {
            return dt.StartOfWeek(DayOfWeek.Monday);
        }
        /// <summary>
        /// Gets the DayOfWeek of this DateTime @ midnight
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="startOfWeek">Specified DayOfWeek used for start of week</param>
        /// <returns>DateTime @ midnight of specified DayOfWeek</returns>
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }

            return dt.AddDays(-1 * diff).Date;
        }
    }
}
