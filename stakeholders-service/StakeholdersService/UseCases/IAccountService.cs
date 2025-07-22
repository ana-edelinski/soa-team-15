using FluentResults;
using StakeholdersService.Dtos;

namespace StakeholdersService.UseCases
{
    public interface IAccountService
    {
        Result<PagedResult<AccountDto>> GetPagedAccount(int page, int pageSize);
        Result<AccountDto> BlockUser(AccountDto account);

        Result<UserProfileDto> GetPersonByUserId(long userId);
        Result<UserProfileDto> UpdatePerson(UserProfileDto profile);


    }
}
