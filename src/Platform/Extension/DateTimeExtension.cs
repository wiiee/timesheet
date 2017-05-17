namespace Platform.Extension
{
    using System;

    public static class DateTimeExtension
    {
        private const string COMMON_FORMAT = "yyyy/MM/dd";
        private const string TIME_SHEET_FORMAT = "yyyy/MM/dd";

        public static string GetUiDate(this DateTime date)
        {
            return date.ToString(COMMON_FORMAT);
        }

        public static string GetTimeSheetId(this DateTime dateTime)
        {
            return dateTime.ToString(TIME_SHEET_FORMAT);
        }

        public static DateTime GetMonday(this DateTime dateTime)
        {
            int interval = 1 - (int)dateTime.DayOfWeek;
            interval = interval == 1 ? -6 : interval;
            return dateTime.AddDays(interval);
        }

        public static int GetDayIndex(this DateTime dateTime)
        {
            int index = (int)dateTime.DayOfWeek;
            return index > 0 ? index - 1 : 6;
        }

        public static bool IsEmpty(this DateTime dateTime)
        {
            return dateTime == null ? true : dateTime == DateTime.MinValue;
        }
    }
}
