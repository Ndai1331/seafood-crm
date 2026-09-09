using System;

namespace Core.Helper
{
    public static class RandomCodeHelper
    {
        public static string GenerateRandomCode(int startRange, int startEnd, int codeCount)
        {
            Random rnd = new Random();
            var code = "";

            for (int i = 0; i < codeCount; i++)
            {
               code += rnd.Next(startRange, startEnd).ToString();
            }

            return code;
        }
    }
}