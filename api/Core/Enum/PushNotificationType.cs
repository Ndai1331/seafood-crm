using System.ComponentModel;

namespace Core.Enum
{
    public enum PushNotificationType
    {
        [Description("Mobile App")]
        MobileApp = 0,
        [Description("SMS")]
        SMS = 1,
        [Description("Zalo OA")]
        ZaloOA = 2,
        [Description("Call")]
        Call = 3,
        [Description("Firebase")]
        Firebase = 4,
    }
}