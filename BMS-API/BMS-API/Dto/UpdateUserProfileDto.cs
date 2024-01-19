namespace BMS.Dto
{
    public class UpdateUserProfileDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmPassword { get; set; }
    }
}
