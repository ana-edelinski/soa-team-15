namespace StakeholdersService.Domain
{
    public class User
    {
        public long Id { get; set; } 
        public string Username { get; set; }
        public string Password { get; set; }
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }

        public User() { }

        public User(string username, string password, UserRole role, bool isActive)
        {
            Username = username;
            Password = password;
            Role = role;
            IsActive = isActive;
            Validate();
        }

        private void Validate()
        {
            if (string.IsNullOrWhiteSpace(Username)) throw new ArgumentException("Invalid Username");
            if (string.IsNullOrWhiteSpace(Password)) throw new ArgumentException("Invalid Password");
        }

        //public string GetPrimaryRoleName()
        //{
        //    return Role.ToString().ToLower();
        //}

        public string GetPrimaryRoleName()
        {
            return Role switch
            {
                UserRole.Administrator => "Administrator",
                UserRole.Tourist => "Tourist",
                UserRole.TourAuthor => "TourAuthor",
                _ => "Unknown"
            };

        }

    }

    public enum UserRole
    {
        Administrator,
        TourAuthor,
        Tourist
    }
}
