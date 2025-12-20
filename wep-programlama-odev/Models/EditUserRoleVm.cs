using System.ComponentModel.DataAnnotations;

namespace wep_programlama_odev.Models
{
    public class EditUserRoleVm
    {
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";

        public List<string> AllRoles { get; set; } = new();

        [Required(ErrorMessage = "Rol seçiniz.")]
        public string? SelectedRole { get; set; }
    }
}
