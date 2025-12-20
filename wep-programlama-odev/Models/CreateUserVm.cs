using System.ComponentModel.DataAnnotations;

namespace wep_programlama_odev.Models
{
    public class CreateUserVm
    {
        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta girin.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rol seçilmelidir.")]
        [Display(Name = "Rol")]
        public string Role { get; set; } = string.Empty;
    }
}
