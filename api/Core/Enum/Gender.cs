using System.ComponentModel;

namespace Core.Enum
{
    public enum Gender
    {
        [Description("Female")]
        Female = 0,
        [Description("Male")]
        Male = 1,
        [Description("Unknown")]
        Unknown = 2
    }
}