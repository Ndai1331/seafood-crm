using System;

namespace Contract.Identity.RoleManager
{
    public class RoleClaimDto
    {
        public int Id { get; set; }
        
        public int RoleId { get; set; }
        
        public  string ClaimType { get; set; }
        
        public  string ClaimValue { get; set; }
    }
}