using System.ComponentModel.DataAnnotations;

namespace BMS.Model
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200), EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;


    }
}
