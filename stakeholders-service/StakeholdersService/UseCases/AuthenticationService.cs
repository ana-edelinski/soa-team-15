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
                // Parse role from input (Tourist or Guide)
                var role = Enum.Parse<UserRole>(account.Role, true);

                var user = _userRepository.Create(new User(
                    account.Username,
                    account.Password,
                role,
                    true));

                var person = _personRepository.Create(new Person(
                    user.Id,
                    account.Name,
                    account.Surname,
                    account.Email,
                    account.ProfilePicture,
                    account.Biography,
                    account.Motto,
                    account.Wallet));

                return _tokenGenerator.GenerateAccessToken(user, person.Id);
            }
            catch (ArgumentException e)
            {
                return Result.Fail(e.Message);
            }
        }
    }
}
