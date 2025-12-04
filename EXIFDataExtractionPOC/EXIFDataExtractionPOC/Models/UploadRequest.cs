using Microsoft.AspNetCore.Http;

namespace EXIFDataExtractionPOC.Models
{
    public class UploadRequest
    {
        public IFormFile Image { get; set; } = null!;
        public string? DeviceLatitude { get; set; }
        public string? DeviceLongitude { get; set; }
    }
}