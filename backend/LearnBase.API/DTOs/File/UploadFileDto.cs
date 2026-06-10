using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace LearnBase.API.DTOs.File
{
    public class UploadFileDto
    {
        [Required]
        public IFormFile File { get; set; } = null!;

        [Required]
        public Guid LessonId { get; set; }
    }
}