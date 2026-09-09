using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Helper
{
    public class StorageCodeHelper
    {
        public static string GetStorageCodeFromConfigCode(string configCode)
        {
            var tags = configCode.Split(new[] { '[', ']' });
            var storageCode = "";

            foreach (string item in tags)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    var ticket = item;
                    ticket = item.ToUpper();
                    switch (ticket)
                    {
                        case "YEAR":
                        {
                            storageCode += $"{DateTime.Now.Year}";
                            break;
                        }
                        case "MONTH":
                        {
                            storageCode += $"{DateTime.Now.Month}";
                            break;
                        }
                        case "DAY":
                        {
                            storageCode +=
                                $"{DateTime.Now.Day}{DateTime.Now.Hour}{DateTime.Now.Millisecond}";
                            break;
                        }

                        case "RANDOMCODE":
                        {
                            storageCode += RandomCodeHelper.GenerateRandomCode(0, 10, 4);
                            break;
                        }
                        default:
                        {
                            storageCode += ticket;
                            break;
                        }
                    }
                }
            }

            return storageCode;
        }
    }
}
