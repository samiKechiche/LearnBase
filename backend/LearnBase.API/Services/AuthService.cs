using LearnBase.API.Data;
using LearnBase.API.DTOs.Auth;
using LearnBase.API.DTOs.Shared;
using LearnBase.API.Models;
using Microsoft.EntityFrameworkCore;

namespace LearnBase.API.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;

        public AuthService(ApplicationDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<ApiResponseDto<AuthResponseDto>> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users
            .FirstOrDefaultAsync(e => e.Email == loginDto.Email);

            if (user == null)
            {
                return ApiResponseDto<AuthResponseDto>.ErrorResponse("User with Email not found");
            }

            if (!PasswordHasherService.VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                return ApiResponseDto<AuthResponseDto>.ErrorResponse("Invalid Password");
            }

            var token = _jwtService.GenerateToken(user.UserId, user.Username);
            return ApiResponseDto<AuthResponseDto>.SuccessResponse(MapToAuthResponseDto(user, token));
        }

        // registration
        // seperating user creation and user saving to use the data annotation validators in controller
        public User CreateUserFromRegisterDto(RegisterDto registerDto)
        {
            var hashedPassword = PasswordHasherService.HashPassword(registerDto.Password);
            return new User
            {
                Email = registerDto.Email,
                Username = registerDto.Username,
                PasswordHash = hashedPassword
            };
        }

        public async Task<ApiResponseDto<AuthResponseDto>> SaveUserToDatabase(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();


            var token = _jwtService.GenerateToken(user.UserId, user.Username);
            return ApiResponseDto<AuthResponseDto>.SuccessResponse
                (
                    MapToAuthResponseDto(user, token),
                    "User Created Successfully."
                );
        }


        private AuthResponseDto MapToAuthResponseDto(User user, string token)
        {
            var dto = new AuthResponseDto
            {
                Token = token,
                UserId = user.UserId,
                //Username = user.Username,
                //ProfilePicture = user.ProfilePicture,
                //Email = user.Email,
                //PasswordHash = user.PasswordHash,
                //PasswordUpdatedAt = user.PasswordUpdatedAt,
                //CreatedAt = user.CreatedAt,
            };

            return dto;
        }
    }
}
