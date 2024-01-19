namespace BMS.Dto
{
    public class DeviceReadingsDto
    {
        public int DeviceId { get; set; }
        public string DeviceName { get; set; }

        public string Status { get; set; }
        public Dictionary<string, string> LatestReadings { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, double> AverageReadings { get; set; } = new Dictionary<string, double>();
    }
}
