using System.ComponentModel;

namespace Core.Extension
{
	public static class EnumExtensions
    {
        public static string GetDescriptionOrName<T>(this T enumValue) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                return null;

            var result = enumValue.ToString();

            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString() ?? string.Empty);

            if (fieldInfo != null)
            {
                var attrs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), true);
                if (attrs.Length > 0)
                {
                    result = ((DescriptionAttribute)attrs[0]).Description;
                }
            }
            else
            {
                result = System.Enum.GetName(typeof(T), enumValue);
            }

            return result;
        }
        
    }
}