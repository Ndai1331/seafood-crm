using System.ComponentModel;

namespace Core.Enum
{
    public enum RelationshipEnum
    {
        [Description("Con")]
        Child,
        [Description("Vợ")]
        Wife,
        [Description("Chồng")]
        Husband,
        [Description("Ba")]
        Father,
        [Description("Mẹ")]
        Mother,
        [Description("Ông")]
        Grandfather,
        [Description("Bà")]
        Grandmother,
        [Description("Khác")]
        Other
    }
}
