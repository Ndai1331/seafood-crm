namespace Core.Helper
{
    public static class DateTimeExtensions
    {
        public static DateTime ExtToStartOfDay(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, 0);
        }

        public static DateTime? ExtToStartOfDay(this DateTime? dateTime)
        {
            if (dateTime == null)
            {
                return null;
            }
            else
            {
                DateTime dateTimeLoad = (DateTime)(dateTime == null ? DateTime.Now : dateTime);
                return new DateTime(
                    dateTimeLoad.Year,
                    dateTimeLoad.Month,
                    dateTimeLoad.Day,
                    23,
                    59,
                    59,
                    999
                );
            }
        }

        /// <summary>
        /// Gönderilen Tarihin bitiş zamanını verir.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime ExtToEndOfDay(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59, 999);
        }

        public static DateTime? ExtToEndOfDay(this DateTime? dateTime)
        {
            if (dateTime == null)
            {
                return null;
            }
            else
            {
                DateTime dateTimeLoad = (DateTime)(dateTime == null ? DateTime.Now : dateTime);
                return new DateTime(
                    dateTimeLoad.Year,
                    dateTimeLoad.Month,
                    dateTimeLoad.Day,
                    23,
                    59,
                    59,
                    999
                );
            }
        }
    }
}