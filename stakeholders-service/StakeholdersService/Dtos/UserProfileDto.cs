namespace StakeholdersService.Dtos
{
    public class UserProfileDto
    {
        public long UserId { get; set; }
        public string? UserName { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Biography { get; set; }
        public string? Motto { get; set; }
        public string? ProfileImagePath { get; set; }
        public string? ImageBase64 { get; set; }
    }
}
