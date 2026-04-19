namespace LearnBase.API.DTOs.Auth
{
    public class ChangePasswordDto
    {
        public string Token {  get; set; }
        public string Current { get; set; }
        public string New { get; set; }
    }
}
