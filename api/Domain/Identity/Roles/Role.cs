using System;
using Microsoft.AspNetCore.Identity;

namespace Domain.Identity.Roles

{
    public class Role : IdentityRole<int>
    {
        public string? Code { get; set; }
        public string? DefaultPageUrl { get; set; }
    }
}