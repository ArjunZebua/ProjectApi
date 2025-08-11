using System.ComponentModel.DataAnnotations;

namespace API.Model
{
    //RefreshTokenRequestDto
    public class RefreshToken
    {
        public int Id { get; set; }
        
        [Required]
        public string Token { get; set; } = string.Empty;
        
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        
        public DateTime CreatedAt { get; set; }
        public DateTime Expires { get; set; }
        public DateTime? RevokedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}