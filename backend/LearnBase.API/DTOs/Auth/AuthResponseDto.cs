namespace LearnBase.API.DTOs.Auth
{
    public class AuthResponseDto
    {
        public Guid UserId { get; set; }
        //public string Username { get; set; }
        //public string? ProfilePicture { get; set; }
        //public string Email { get; set; }
        //public string PasswordHash { get; set; }
        //public DateTime PasswordUpdatedAt { get; set; }
        //public DateTime CreatedAt { get; set; }
        public string Token { get; set; }
    }
}
