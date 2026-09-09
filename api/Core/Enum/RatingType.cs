using System.ComponentModel;

namespace Core.Enum
{
    public enum RatingType
    {
        [Description("Unknown")]Unknown = 0,
        [Description("1 Star")] Bad = 1,
        [Description("2 Star")] Normal = 2,
        [Description("3 Star")] NotBad = 3,
        [Description("4 Star")] Good = 4,
        [Description("5 Star")] VeryGood = 5,
    }
}