using FluentResults;
using StakeholdersService.Dtos;

namespace StakeholdersService.UseCases
{
    public interface IAccountService
    {
        Result<PagedResult<AccountDto>> GetPagedAccount(int page, int pageSize);
        Result<AccountDto> BlockUser(AccountDto account);

        UserProfileDto GetPersonByUserId(long userId);
        void UpdatePerson(UserProfileDto profile, long userId);


    }
}
