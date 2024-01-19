namespace BMS.Dto
{
    public class DeviceCapabilityDto
    {
        
        public int Id { get; set; }
        public string CapabilityType { get; set; }

        public DeviceDto Device { get; set; }
        
    }
}