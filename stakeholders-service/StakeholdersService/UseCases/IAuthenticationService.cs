using StakeholdersService.Dtos;
using FluentResults;

namespace StakeholdersService.UseCases
{
    public interface IAuthenticationService
    {
        Result<AuthenticationTokensDto> RegisterTourist(AccountRegistrationDto account);
        Result<AuthenticationTokensDto> Login(CredentialsDto credentials);

    }
}
