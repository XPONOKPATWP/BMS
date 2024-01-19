using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BMS.Model
{
    public class Hub
    {
        [Key]
        public int Id { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public string SerialNumber { get; set; } = null!;

        [Required]
        public string Name { get; set; } = null!;

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public int RoomId { get; set; }

        [ForeignKey("RoomId")]
        public virtual Room Room { get; set; } = null!;

        public virtual List<Device> Devices { get; set; } = new List<Device>();
    }
}