using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Identity.Users;

namespace Domain.Positions
{
    // Explicit table mapping: EF default would use DbSet name "Positions" (PascalCase),
    // which breaks on MySQL instances with lower_case_table_names=0 (e.g. staging clone).
    [Table("positions")]
    public class Position
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string Name { get; set; }
        public string? Note { get; set; }
        public int ODX { get; set; }
        public List<User> Users { get; set; }
    }
}