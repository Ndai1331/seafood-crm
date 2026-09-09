
namespace BootstrapBlazor.Server.Data;
public class RoleDto
{
        public int Id { get; set; }
        public string Name { get; set;}
        public string? Code { get; set; }
}
public class RoleDefaultPageDto
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = "";
    public string? DefaultPageUrl { get; set; }
}

public class RoleClaimDto
{
    public int Id { get; set; }

    public int RoleId { get; set; }

    public  string ClaimType { get; set; }

    public  string ClaimValue { get; set; }
}
