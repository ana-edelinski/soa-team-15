using FluentResults;
using StakeholdersService.Dtos;
using System;
using StakeholdersService.Domain;
using StakeholdersService.Domain.RepositoryInterfaces;

namespace StakeholdersService.UseCases
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPersonRepository _personRepository;
        private readonly ITokenGenerator _tokenGenerator;

        public AuthenticationService(
            IUserRepository userRepository,
            IPersonRepository personRepository,
            ITokenGenerator tokenGenerator)
        {
            _userRepository = userRepository;
            _personRepository = personRepository;
            _tokenGenerator = tokenGenerator;
        }

        public Result<AuthenticationTokensDto> RegisterTourist(AccountRegistrationDto account)
        {
            if (_userRepository.Exists(account.Username))
                return Result.Fail("Username already exists");

            try
            {
                if (!Enum.TryParse<UserRole>(account.Role, true, out var role) ||
                    (role != UserRole.Tourist && role != UserRole.TourAuthor))
                {
                    return Result.Fail("Invalid role. Allowed roles are only 'Tourist' or 'Author'.");
                }

                var user = _userRepository.Create(new User(
                    account.Username,
                    account.Password,
                role, true));

                var person = _personRepository.Create(new Person(
                    user.Id,
                    account.Email));

                return _tokenGenerator.GenerateAccessToken(user, person.Id);
            }
            catch (ArgumentException e)
            {
                return Result.Fail(e.Message);
            }
        }

        public Result<AuthenticationTokensDto> Login(CredentialsDto credentials)
        {
            var user = _userRepository.GetActiveByName(credentials.Username);
            if (user == null || credentials.Password != user.Password)
            {
                return Result.Fail("Invalid username or password.");
            }
            long personId;
            try
            {
                personId = _userRepository.GetPersonId(user.Id);
            }
            catch (KeyNotFoundException)
            {
                personId = 0;
            }
            return _tokenGenerator.GenerateAccessToken(user, personId);
        }
    }
}
