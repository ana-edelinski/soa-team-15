namespace StakeholdersService.Domain
{
    public class User
    {
        public long Id { get; set; } 
        public string Username { get; set; }
        public string Password { get; set; }
        public UserRole Role { get; set; }

        public User() { }

        public User(string username, string password, UserRole role)
        {
            Username = username;
            Password = password;
            Role = role;
            Validate();
        }

        private void Validate()
        {
            if (string.IsNullOrWhiteSpace(Username)) throw new ArgumentException("Invalid Username");
            if (string.IsNullOrWhiteSpace(Password)) throw new ArgumentException("Invalid Password");
        }

        public string GetPrimaryRoleName()
        {
            return Role.ToString().ToLower();
        }
    }

    public enum UserRole
    {
        Administrator,
        TourAuthor,
        Tourist
    }
}
