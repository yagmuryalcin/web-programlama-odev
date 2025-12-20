using System.ComponentModel.DataAnnotations;

namespace wep_programlama_odev.Models
{
    public class CreateUserVm
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = "";

        // "Admin" veya "TeamMember"
        public string? Role { get; set; }
    }
}
