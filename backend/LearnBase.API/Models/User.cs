using System;
using System.ComponentModel.DataAnnotations;

namespace LearnBase.API.Models
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; }

        [Required, MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        public string? ProfilePicture { get; set; } // Storing the path/URL to the image

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime PasswordUpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}