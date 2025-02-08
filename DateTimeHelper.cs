namespace ApiOnLamda
{
    public static class DateTimeHelper
    {
        /// <summary>
        /// Converts a DateTime to the format MM/dd/yyyy
        /// </summary>
        /// <param name="dateTime">The DateTime to format</param>
        /// <returns>A string representation of the date in MM/dd/yyyy format</returns>
        //public static string ConvertToMonthDayYearFormat(DateTime dateTime)
        //{
        //    return dateTime.ToString("MM/dd/yyyy");
        //}

        public static DateOnly ToDateOnly(this DateTime date)
        {
            return new DateOnly(date.Year, date.Month, date.Day);
        }

        public static DateOnly? ToDateOnly(this DateTime? date)
        {
            return date != null ? (DateOnly?)date.Value.ToDateOnly() : null;
        }
    }
}
