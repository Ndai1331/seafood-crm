using System;

namespace Contract.Identity.RoleManager
{
    public class RoleClaimModel
    {
        public int RoleId { get; set; }
        
        public  string ClaimType { get; set; }
        
        public  string ClaimValue { get; set; }
    }
}