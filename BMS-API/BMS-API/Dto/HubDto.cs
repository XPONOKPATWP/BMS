namespace BMS.Dto
{
    public class HubDto
    {
        public int Id { get; set; }
        public string SerialNumber { get; set; }
        public string Name { get; set; }
        public List<DeviceDto>? Devices { get; set; }
    }
}