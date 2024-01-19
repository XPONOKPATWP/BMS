using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BMS.Model
{
    public class DeviceCapability
    {
        [Key]
        public int Id { get; set; }

        public bool IsActive { get; set; } = true;
        public int DeviceId { get; set; }

        [ForeignKey("DeviceId")]
        public virtual Device Device { get; set; } = null!;

        [Required]
        public string CapabilityType { get; set; } = null!;
    }
}