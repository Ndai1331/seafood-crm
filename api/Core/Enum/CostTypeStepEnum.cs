using System.ComponentModel;

namespace Core.Enum
{
    /// <summary>
    /// Step type for cost type steps
    /// </summary>
    public enum CostTypeStepType
    {
        [Description("Base")]
        BASE = 1,
        
        [Description("Multiplier")]
        MULTIPLIER = 2,
        
        [Description("Fixed")]
        FIXED = 3
    }

    /// <summary>
    /// Value data type for step values
    /// </summary>
    public enum CostTypeStepValueDataType
    {
        [Description("Integer")]
        INT = 1,
        
        [Description("Decimal")]
        DECIMAL = 2,
        
        [Description("Enum")]
        ENUM = 3,
        
        [Description("Text")]
        TEXT = 4
    }
}

