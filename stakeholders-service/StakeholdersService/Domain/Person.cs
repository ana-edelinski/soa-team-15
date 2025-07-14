using System.Net.Mail;

namespace StakeholdersService.Domain
{
    public class Person
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Email { get; set; }

        public Person() { }

        public Person(long userId, string email)
        {
            UserId = userId;
            Email = email;

            Validate();
        }

        private void Validate()
        {
            if (UserId == 0) throw new ArgumentException("Invalid UserId");
            if (!MailAddress.TryCreate(Email, out _)) throw new ArgumentException("Invalid Email");
        }
    }
}
