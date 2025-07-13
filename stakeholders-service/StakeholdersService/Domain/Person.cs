using System.Net.Mail;

namespace StakeholdersService.Domain
{
    public class Person
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string? ImageUrl { get; set; }
        public string? Biography { get; set; }
        public string? Motto { get; set; }
        public decimal? Wallet { get; set; }

        public Person() { }

        public Person(long userId, string name, string surname, string email,
            string? imageUrl, string? biography, string? motto, decimal? wallet)
        {
            UserId = userId;
            Name = name;
            Surname = surname;
            Email = email;
            ImageUrl = imageUrl;
            Biography = biography;
            Motto = motto;
            Wallet = wallet;

            Validate();
        }

        private void Validate()
        {
            if (UserId == 0) throw new ArgumentException("Invalid UserId");
            if (string.IsNullOrWhiteSpace(Name)) throw new ArgumentException("Invalid Name");
            if (string.IsNullOrWhiteSpace(Surname)) throw new ArgumentException("Invalid Surname");
            if (!MailAddress.TryCreate(Email, out _)) throw new ArgumentException("Invalid Email");
        }
    }
}
