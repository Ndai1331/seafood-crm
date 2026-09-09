using System;

namespace Core.Extension
{
    public static class DateTimeExtensions
    {
        
        public static string ToDate(this DateTime datetime)
        {
            return  $"{datetime:dd-MM-yyyy}";
        }

        public static uint ToDoUInt32DateTime(this DateTime dateTime)
        {
            DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan currTime = dateTime - startTime;
            uint time_t = Convert.ToUInt32(Math.Abs(currTime.TotalSeconds));
            return time_t;
        }
    }
}