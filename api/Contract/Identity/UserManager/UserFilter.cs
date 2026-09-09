using Core.Enum;

namespace Contract.Identity.UserManager
{
    public class UserCompanyFilter : BaseFilterPagingDto 
    {
    public string? UserName { set; get; } = null;
    public string? UserCode { set; get; } = null;
    public string? FirstName { set; get; } = null;
    public string? LastName { set; get; } = null;
    public int? TeamId { set; get; } = null;
    }
}
