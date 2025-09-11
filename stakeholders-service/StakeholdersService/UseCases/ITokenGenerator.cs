using FluentResults;
using StakeholdersService.Domain;
using StakeholdersService.Dtos;

namespace StakeholdersService.UseCases
{
    public interface ITokenGenerator
    {
        Result<AuthenticationTokensDto> GenerateAccessToken(User user, long personId);
    }
}
