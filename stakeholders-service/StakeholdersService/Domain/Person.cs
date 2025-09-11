using System.Net.Mail;
using System.Xml.Linq;

namespace StakeholdersService.Domain
{
    public class Person
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Email { get; set; }


        public string? Name { get; set; }
        public string? Surname { get; set;  }
        public string? ProfileImagePath { get; set; }
        public string? Biography { get; set; }
        public string? Motto { get; set; }

        public Person() { }

        public Person(long userId, string email)
        {
            UserId = userId;
            Email = email;

            Validate();
        }

        public Person(long userId, string email,string? name, string? surname, string? bio, string? motto, string? imagePath)
        {
            UserId = userId;
            Email = email;
            Name = name;
            Surname = surname;
            Biography = bio;
            Motto = motto;
            ProfileImagePath = imagePath;
        }

        private void Validate()
        {
            if (UserId == 0) throw new ArgumentException("Invalid UserId");
            if (!MailAddress.TryCreate(Email, out _)) throw new ArgumentException("Invalid Email");
        }
    }
}
