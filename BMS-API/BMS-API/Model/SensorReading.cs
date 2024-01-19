using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BMS.Model
{
    public class SensorReading
    {
        [Key]
        public int Id { get; set; }

        public int DeviceId { get; set; }

        [ForeignKey("DeviceId")]
        public virtual Device Device { get; set; } = null!;

        public int DeviceCapabilityId { get; set; }

        [ForeignKey("DeviceCapabilityId")]
        public virtual DeviceCapability DeviceCapability { get; set; } = null!;

        public DateTime Timestamp { get; set; }

        public double Value { get; set; }
    }
}
