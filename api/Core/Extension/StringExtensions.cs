using System.Globalization;

namespace Core.Extension
{
	public static class StringExtensions
	{
        public static string FormatThousand(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "0";
            int number = Convert.ToInt32(value);
            if (number >= 1000000)
            {
                var scaled = number / 1000000.0;
                var text = (Math.Abs(scaled % 1) < 0.000001)
                    ? scaled.ToString("0", CultureInfo.InvariantCulture)
                    : scaled.ToString("0.#", CultureInfo.InvariantCulture);
                return text + "M";
            }
            else if (number >= 1000)
            {
                return (number / 1000.0).ToString("n0") + "K";
            }

            return number.ToString("n0", CultureInfo.CreateSpecificCulture("vi-VN"));
        }
    }
}
