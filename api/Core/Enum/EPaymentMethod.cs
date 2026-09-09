using System.ComponentModel;

namespace Core.Enum
{
    public enum EPaymentMethod
    {
        [Description("Tiền mặt")]
        Cash = 1,
        [Description("Chuyển khoản")]
        BankTransfer,
        [Description("VN Pay")]
        VNPay
    }
}