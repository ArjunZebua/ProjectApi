using API.Model.DTOs;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Registrasi user baru
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value!.Errors.Count > 0)
                        .SelectMany(x => x.Value!.Errors)
                        .Select(x => x.ErrorMessage)
                        .ToList();

                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Data tidak valid",
                        Errors = errors
                    });
                }

                var result = await _authService.RegisterAsync(registerDto);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Register endpoint");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Terjadi kesalahan server",
                    Errors = new List<string> { "Internal server error" }
                });
            }
        }

        /// <summary>
        /// Login user
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value!.Errors.Count > 0)
                        .SelectMany(x => x.Value!.Errors)
                        .Select(x => x.ErrorMessage)
                        .ToList();

                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Data tidak valid",
                        Errors = errors
                    });
                }

                var result = await _authService.LoginAsync(loginDto);

                if (!result.Success)
                {
                    return Unauthorized(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Login endpoint");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Terjadi kesalahan server",
                    Errors = new List<string> { "Internal server error" }
                });
            }
        }

        /// <summary>
        /// Refresh access token
        /// </summary>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.RefreshToken))
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Refresh token wajib diisi",
                        Errors = new List<string> { "Refresh token is required" }
                    });
                }

                var result = await _authService.RefreshTokenAsync(request.RefreshToken);

                if (!result.Success)
                {
                    return Unauthorized(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RefreshToken endpoint");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Terjadi kesalahan server",
                    Errors = new List<string> { "Internal server error" }
                });
            }
        }

        /// <summary>
        /// Logout user
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.RefreshToken))
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Refresh token wajib diisi",
                        Errors = new List<string> { "Refresh token is required" }
                    });
                }

                var result = await _authService.LogoutAsync(request.RefreshToken);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Logout endpoint");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Terjadi kesalahan server",
                    Errors = new List<string> { "Internal server error" }
                });
            }
        }

        /// <summary>
        /// Get informasi user yang sedang login
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "User tidak valid",
                        Errors = new List<string> { "Invalid user" }
                    });
                }

                var user = await _authService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "User tidak ditemukan",
                        Errors = new List<string> { "User not found" }
                    });
                }

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

                return Ok(new ApiResponseDto<UserDto>
                {
                    Success = true,
                    Message = "Data user berhasil diambil",
                    Data = userDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCurrentUser endpoint");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Terjadi kesalahan server",
                    Errors = new List<string> { "Internal server error" }
                });
            }
        }

        /// <summary>
        /// Validasi token
        /// </summary>
        [HttpGet("validate-token")]
        [Authorize]
        public IActionResult ValidateToken()
        {
            try
            {
                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Token valid",
                    Data = new
                    {
                        UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        Username = User.FindFirst(ClaimTypes.Name)?.Value,
                        Email = User.FindFirst(ClaimTypes.Email)?.Value,
                        Role = User.FindFirst(ClaimTypes.Role)?.Value
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ValidateToken endpoint");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Terjadi kesalahan server",
                    Errors = new List<string> { "Internal server error" }
                });
            }
        }
    }
}