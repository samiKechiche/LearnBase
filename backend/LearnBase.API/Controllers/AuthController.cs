using LearnBase.API.DTOs.Auth;
using LearnBase.API.DTOs.Shared;
using LearnBase.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace LearnBase.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("/login")]
        public async Task<ActionResult<ApiResponseDto<AuthResponseDto>>> Login([FromBody] LoginDto loginDto)
        {
            var result = await _authService.LoginAsync(loginDto);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("/register")]
        public async Task<ActionResult<ApiResponseDto<AuthResponseDto>>> Register([FromBody] RegisterDto registerDto)
        {
            var user = _authService.CreateUserFromRegisterDto(registerDto);

            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}
            ModelState.ClearValidationState(nameof(user));
            if (!TryValidateModel(user, nameof(user)))
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.SaveUserToDatabase(user);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
