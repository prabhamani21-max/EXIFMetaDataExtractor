using Microsoft.AspNetCore.Mvc;
using EXIFDataExtractionPOC.Models;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System.Globalization;

namespace EXIFDataExtractionPOC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly ILogger<ImageController> _logger;

        public ImageController(ILogger<ImageController> logger)
        {
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] UploadRequest request)
        {
            if (request.Image == null || request.Image.Length == 0)
            {
                return BadRequest("No image uploaded.");
            }

            try
            {
                // Extract EXIF GPS
                var location = await ExtractGpsFromImage(request.Image);

                if (location != null)
                {
                    return Ok(location);
                }

                // Fallback to device GPS
                if (!string.IsNullOrEmpty(request.DeviceLatitude) && !string.IsNullOrEmpty(request.DeviceLongitude))
                {
                    if (double.TryParse(request.DeviceLatitude, NumberStyles.Float, CultureInfo.InvariantCulture, out double lat) &&
                        double.TryParse(request.DeviceLongitude, NumberStyles.Float, CultureInfo.InvariantCulture, out double lng))
                    {
                        return Ok(new LocationData
                        {
                            Latitude = lat,
                            Longitude = lng,
                            Source = "Device"
                        });
                    }
                }

                return BadRequest("No GPS data available from image or device.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing image upload.");
                return StatusCode(500, "Internal server error.");
            }
        }

        private async Task<LocationData?> ExtractGpsFromImage(IFormFile image)
        {
            using var stream = image.OpenReadStream();
            var directories = ImageMetadataReader.ReadMetadata(stream);

            var gpsDirectory = directories.OfType<GpsDirectory>().FirstOrDefault();
            if (gpsDirectory != null)
            {
                if (gpsDirectory.TryGetGeoLocation(out var geoLocation))
                {
                    return new LocationData
                    {
                        Latitude = geoLocation.Latitude,
                        Longitude = geoLocation.Longitude,
                        Source = "EXIF"
                    };
                }
            }

            return null;
        }
    }
}