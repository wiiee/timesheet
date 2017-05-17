namespace Platform.Util
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Extension;

    public static class DateTimeUtil
    {
        public static List<Item> GetRecentMonths(int monthNumbers, string format)
        {
            var items = new List<Item>();
            var today = DateTime.Today;
            int i = 0;

            while(i++ < monthNumbers)
            {
                var item = new Item();
                item.Key = today.Year + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(today.Month);
                item.Value = today.ToString(format);

                items.Add(item);

                today = today.AddMonths(1);
            }

            return items;
        }

        public static DateTime GetCurrentMonday()
        {
            var today = DateTime.Today;
            int interval = 1 - (int)today.DayOfWeek;
            interval = interval == 1 ? -6 : interval;
            return today.AddDays(interval);
        }

        //机票周报周期是上周五到本周四
        public static DateTime GetCurrentPeriodStartDay()
        {
            var today = DateTime.Today;
            int interval = 5- (int)today.DayOfWeek;
            if (interval > 0) interval -= 7;
            return today.AddDays(interval);
        }

        public static DateTime GetCurrentWeekEndDay()
        {
            return DateTime.Today.AddDays(7 - (int)DateTime.Today.DayOfWeek);
        }

        public static DateTime GetCurrentMonthStartDay()
        {
            DateTime date = DateTime.Today;
            return new DateTime(date.Year, date.Month, 1);
        }

        public static DateTime GetCurrentMonthEndDay()
        {
            DateTime date = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            return date.AddMonths(1).AddDays(-1);
        }

        public static int GetWorkingDays(DateTime startDate, DateTime endDate)
        {
            var firstDate = new DateTime(startDate.Ticks);
            var result = 0;

            while(firstDate <= endDate)
            {
                if (firstDate.DayOfWeek != DayOfWeek.Saturday && firstDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    result++;
                }

                firstDate = firstDate.AddDays(1);
            }

            return result;
        }

        public static DateTime GetNextMonthStartDay(DateTime date)
        {
            var year = date.Year;
            var month = date.Month + 1;
            if(month > 12)
            {
                month = month - 12;
                year++;
            }
            return new DateTime(year, month, 1);
        }

        public static DateTime GetNextMonthEndDay(DateTime date)
        {
            var year = date.Year;
            var month = date.Month + 2;
            if (month > 12)
            {
                month = month - 12;
                year++;
            }
            return new DateTime(year, month, 1).AddDays(-1);
        }
    }
}
