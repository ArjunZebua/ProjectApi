using System.ComponentModel.DataAnnotations;

namespace API.Model.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Username atau Email wajib diisi")]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password wajib diisi")]
        [MinLength(6, ErrorMessage = "Password minimal 6 karakter")]
        public string Password { get; set; } = string.Empty;
    }
}