using System.ComponentModel.DataAnnotations;

namespace Contract.Teams
{
    public class UpdateTeamDto
    {
        [Required(ErrorMessage = "Id is required")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Code is required")]
        [StringLength(50, ErrorMessage = "Code must not exceed 50 characters")]
        public string Code { get; set; } = string.Empty;

        [StringLength(150, ErrorMessage = "Name must not exceed 150 characters")]
        public string? Name { get; set; }
    }
}

