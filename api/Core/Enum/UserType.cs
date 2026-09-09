using System.ComponentModel;

namespace Core.Enum
{
    public enum UserType
    {
        [Description("Inhouse")]
        Inhouse = 0,
        [Description("Remote")]
        Remote = 1,
        [Description("OS Z")]
        OSZ = 2,
        [Description("OS")]
        OS = 3
    }
}
