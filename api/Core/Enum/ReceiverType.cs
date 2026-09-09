using System.ComponentModel;

namespace Core.Enum
{
    public enum ReceiverType
    {
        [Description("Thực hiện")]
        Processer = 1,
        [Description("Theo dõi")]
        NotificationRecipient,
    }
}
