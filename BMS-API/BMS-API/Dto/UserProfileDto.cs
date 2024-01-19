using System.ComponentModel.DataAnnotations;

namespace BMS.Dto
{
    public class UserProfileDto
    {
        public string Email { get; set; } = null!;
        public string Name { get; set; } = null!;
    }
}
