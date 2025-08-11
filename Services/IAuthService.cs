using API.Model;
using API.Model.DTOs;

namespace API.Services
{
    public interface IAuthService
    {
        Task<ApiResponseDto<AuthResponseDto>> RegisterAsync(RegisterDto registerDto);
        Task<ApiResponseDto<AuthResponseDto>> LoginAsync(LoginDto loginDto);
        Task<ApiResponseDto<AuthResponseDto>> RefreshTokenAsync(string refreshToken);
        Task<ApiResponseDto<bool>> LogoutAsync(string refreshToken);
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByEmailAsync(string email);
    }
}