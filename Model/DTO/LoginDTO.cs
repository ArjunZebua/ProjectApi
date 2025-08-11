using System.ComponentModel.DataAnnotations;

namespace API.Model.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Username atau email wajib diisi")]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password wajib diisi")]
        public string Password { get; set; } = string.Empty;
    }
}