using System.ComponentModel;

namespace Core.Enum
{
    public enum TelegramPostContentStatus
    {
        [Description("NeedImprove")] NeedImprove = 1,
        [Description("Good")] Good = 2,
        [Description("Bad")] Bad = 3,
    }
}