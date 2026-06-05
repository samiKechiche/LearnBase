namespace LearnBase.API.DTOs
{
    // Simple form model so Swagger can generate multipart/form-data schema
    public class FileUploadDto
    {
        public IFormFile? File { get; set; }
    }
}