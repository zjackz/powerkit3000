using Microsoft.AspNetCore.Http;

namespace HappyTools.WebApp.DTOs
{
    public class FileUploadRequest
    {
        public IFormFile? File { get; set; }
    }
}
