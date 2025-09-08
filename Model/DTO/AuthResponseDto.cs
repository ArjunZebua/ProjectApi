namespace API.Model.DTOs
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
        public string TokenType { get; set; } = "Bearer";
        public UserDto User { get; set; } = null!;

    }
}