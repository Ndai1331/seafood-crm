using Core.Enum;

namespace Contract.Identity.UserManager
{
    public class UserFilterPagingModel : BaseFilterPagingDto
    {
        public string? FullName { get; set; }
        public string? UserCode { get; set; }
        public string? RoleName { get; set; }
        public int? CreatedBy { get; set; }
    public List<int>? DepartmentIds { get; set; }
    public List<int>? UserIds { get; set; }
    public List<int>? RoleIds { get; set; }
    public List<string>? Phones { get; set; }
    public int? TeamId { get; set; }
    public Gender? Gender{ get; set; }
        public DateTime? DobFrom { get; set; }
        public DateTime? DobTo { get; set; }
    }
}
