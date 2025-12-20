using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace wep_programlama_odev.Models
{
    public class EditUserRoleVm
    {
        [Required]
        public string UserId { get; set; } = default!;

        public string? UserName { get; set; }
        public string? Email { get; set; }

        // Seçili rol
        [Required(ErrorMessage = "Lütfen bir rol seçiniz.")]
        public string? SelectedRole { get; set; }

        // Dropdown için tüm roller
        public List<string> AllRoles { get; set; } = new();
    }
}
