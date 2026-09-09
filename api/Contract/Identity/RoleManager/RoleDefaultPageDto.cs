namespace Contract.Identity.RoleManager
{
    public class RoleDefaultPageDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = "";
        public string? DefaultPageUrl { get; set; }
    }
}
