using API.Model;
using API.Model.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(ApplicationContext context, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }



        public async Task<ApiResponseDto<AuthResponseDto>> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // Cek apakah username sudah ada
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == registerDto.Username || u.Email == registerDto.Email);

                if (existingUser != null)
                {
                    return new ApiResponseDto<AuthResponseDto>
                    {
                        Success = false,
                        Message = "Username atau email sudah terdaftar",
                        Errors = new List<string> { "User already exists" }
                    };
                }

                // Hash password
                string passwordHash = HashPassword(registerDto.Password);

                // Buat user baru
                var user = new User
                {
                    Username = registerDto.Username,
                    Email = registerDto.Email,
                    PasswordHash = passwordHash,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    Role = "User"
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Generate tokens
                var tokens = await GenerateTokensAsync(user);

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                };

                var authResponse = new AuthResponseDto
                {
                    Token = tokens.AccessToken,
                    RefreshToken = tokens.RefreshToken,
                    Expires = tokens.Expires,
                    User = userDto
                };

                return new ApiResponseDto<AuthResponseDto>
                {
                    Success = true,
                    Message = "Registrasi berhasil",
                    Data = authResponse
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return new ApiResponseDto<AuthResponseDto>
                {
                    Success = false,
                    Message = "Terjadi kesalahan saat registrasi",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<AuthResponseDto>> LoginAsync(LoginDto loginDto)
        {
            try
            {
                // Cari user berdasarkan username atau email
                var user = await _context.Users
                    .FirstOrDefaultAsync(u =>
                        (u.Username == loginDto.UsernameOrEmail || u.Email == loginDto.UsernameOrEmail)
                        && u.IsActive);

                if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    return new ApiResponseDto<AuthResponseDto>
                    {
                        Success = false,
                        Message = "Username/email atau password salah",
                        Errors = new List<string> { "Invalid credentials" }
                    };
                }

                // Update last login
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Generate tokens
                var tokens = await GenerateTokensAsync(user);

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                };

                var authResponse = new AuthResponseDto
                {
                    Token = tokens.AccessToken,
                    RefreshToken = tokens.RefreshToken,
                    Expires = tokens.Expires,
                    User = userDto
                };

                return new ApiResponseDto<AuthResponseDto>
                {
                    Success = true,
                    Message = "Login berhasil",
                    Data = authResponse
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return new ApiResponseDto<AuthResponseDto>
                {
                    Success = false,
                    Message = "Terjadi kesalahan saat login",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<AuthResponseDto>> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var refreshTokenEntity = await _context.RefreshTokens
                    .Include(rt => rt.User)
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.IsActive);

                if (refreshTokenEntity == null || refreshTokenEntity.Expires < DateTime.UtcNow)
                {
                    return new ApiResponseDto<AuthResponseDto>
                    {
                        Success = false,
                        Message = "Refresh token tidak valid atau sudah kedaluwarsa",
                        Errors = new List<string> { "Invalid refresh token" }
                    };
                }

                // Deaktivasi refresh token lama
                refreshTokenEntity.IsActive = false;
                refreshTokenEntity.RevokedAt = DateTime.UtcNow;

                // Generate tokens baru
                var tokens = await GenerateTokensAsync(refreshTokenEntity.User);

                var userDto = new UserDto
                {
                    Id = refreshTokenEntity.User.Id,
                    Username = refreshTokenEntity.User.Username,
                    Email = refreshTokenEntity.User.Email,
                    FirstName = refreshTokenEntity.User.FirstName,
                    LastName = refreshTokenEntity.User.LastName,
                    Role = refreshTokenEntity.User.Role,
                    CreatedAt = refreshTokenEntity.User.CreatedAt
                };

                var authResponse = new AuthResponseDto
                {
                    Token = tokens.AccessToken,
                    RefreshToken = tokens.RefreshToken,
                    Expires = tokens.Expires,
                    User = userDto
                };

                await _context.SaveChangesAsync();

                return new ApiResponseDto<AuthResponseDto>
                {
                    Success = true,
                    Message = "Token berhasil diperbarui",
                    Data = authResponse
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return new ApiResponseDto<AuthResponseDto>
                {
                    Success = false,
                    Message = "Terjadi kesalahan saat memperbarui token",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<bool>> LogoutAsync(string refreshToken)
        {
            try
            {
                var refreshTokenEntity = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

                if (refreshTokenEntity != null)
                {
                    refreshTokenEntity.IsActive = false;
                    refreshTokenEntity.RevokedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                return new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Logout berhasil",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Terjadi kesalahan saat logout",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username && u.IsActive);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
        }

        private async Task<(string AccessToken, string RefreshToken, DateTime Expires)> GenerateTokensAsync(User user)
        {
            // Generate Access Token (JWT)
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JWT:Secret"] ?? "YourSecretKeyHere");
            var expires = DateTime.UtcNow.AddHours(1); // Token berlaku 1 jam

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("FirstName", user.FirstName),
                    new Claim("LastName", user.LastName)
                }),
                Expires = expires,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["JWT:Issuer"],
                Audience = _configuration["JWT:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(token);

            // Generate Refresh Token
            var refreshToken = GenerateRefreshToken();

            // Simpan refresh token ke database
            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(7), // Refresh token berlaku 7 hari
                IsActive = true
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            return (accessToken, refreshToken, expires);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}