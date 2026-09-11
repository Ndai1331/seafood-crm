using BootstrapBlazor.Server.Helper;
using Xunit;

namespace BootstrapBlazor.Server.Tests;

public class VnNumberFormatHelperTests
{
    [Fact]
    public void Format_UsesVietnameseThousandsAndDecimalSeparators()
    {
        Assert.Equal("1.234.567,5", VnNumberFormatHelper.Format(1_234_567.5m));
    }

    [Fact]
    public void FormatInt_UsesVietnameseThousandsSeparator()
    {
        Assert.Equal("1.234.567", VnNumberFormatHelper.FormatInt(1_234_567));
    }

    [Fact]
    public void FormatWhileTyping_PreservesDecimalAndNegativeValues()
    {
        Assert.Equal("-1.234,56", VnNumberFormatHelper.FormatWhileTyping("-1234,56"));
    }

    [Fact]
    public void ParseDecimal_ParsesFormattedVietnameseValue()
    {
        Assert.Equal(1_234_567.5m, VnNumberFormatHelper.ParseDecimal("1.234.567,5"));
    }
}
