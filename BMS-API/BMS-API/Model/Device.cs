using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BMS.Model
{
    public class Device
    {
        [Key]
        public int Id { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public string DeviceType { get; set; } = null!;

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string SerialNumber { get; set; } = null!;

        public int HubId { get; set; }

        [ForeignKey("HubId")]
        public virtual Hub Hub { get; set; } = null!;

        public int RoomId { get; set; }

        [ForeignKey("RoomId")]
        public virtual Room Room { get; set; } = null!;

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public string Status { get; set; } = null!;

        public string FailureDescription { get; set; } = null!;

        public virtual List<DeviceCapability> DeviceCapabilities { get; set; } = null!;
    }
}