namespace EXIFDataExtractionPOC.Models
{
    public class LocationData
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Source { get; set; } // "EXIF" or "Device"
    }
}