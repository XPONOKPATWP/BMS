namespace BMS.Dto
{
    public class DeviceDto
    {

        public int Id { get; set; }
        public string DeviceType { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string FailureDescription { get; set; }
        public HubDto Hub { get; set; }
        public RoomDto Room { get; set; }

        public List<DeviceCapabilityDto>? DeviceCapabilities { get; set; }

    }
}