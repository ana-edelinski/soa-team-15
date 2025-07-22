using FluentResults;
using AutoMapper;
using StakeholdersService.Domain;
using StakeholdersService.Domain.RepositoryInterfaces;
using StakeholdersService.Dtos;

namespace StakeholdersService.UseCases
{
    public class AccountService : IAccountService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IMapper _mapper;

        public AccountService(IUserRepository userRepository, IPersonRepository personRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _personRepository = personRepository;
            _mapper = mapper;
        }

        private Result<PagedResult<AccountDto>> GetPaged(int page, int pageSize)
        {
            var users = _userRepository.GetPaged(page, pageSize, out int totalCount);

            var userDtos = users.Select(u => _mapper.Map<AccountDto>(u)).ToList();

            return Result.Ok(new PagedResult<AccountDto>(userDtos, totalCount));
        }

        public Result<PagedResult<AccountDto>> GetPagedAccount(int page, int pageSize)
        {
            var mappedResults = GetPaged(page, pageSize);

            if (mappedResults.IsFailed)
            {
                return Result.Fail(mappedResults.Errors);
            }

            var pagedAccounts = mappedResults.Value;
            Person? personResult;
            AccountDto? userResult;

            var personList = _personRepository.GetPaged(0, 0).Results;

            foreach (var account in pagedAccounts.Results)
            {
                try
                {
                    userResult = mappedResults.Value.Results.Find(x => x.Username == account.Username);
                    personResult = personList.Find(p => p.UserId == userResult.Id);

                    account.Email = personResult?.Email ?? "N/A";
                }
                catch (Exception ex)
                {
                    if (account.Role != UserRole.Administrator.ToString())
                    {
                        return Result.Fail($"An error occurred while retrieving the person for account " +
                                           $"{account.Id}: {ex.Message}");
                    }

                    account.Email = "N/A";
                    continue;
                }
            }

            return Result.Ok(pagedAccounts);
        }

        private Result<AccountDto> Update(AccountDto account)
        {
            try
            {
                var user = _mapper.Map<User>(account);

                var updatedUser = _userRepository.Update(user);

                var updatedAccount = _mapper.Map<AccountDto>(updatedUser);

                return Result.Ok(updatedAccount);
            }
            catch (KeyNotFoundException e)
            {
                return Result.Fail(e.Message);
            }
            catch (ArgumentException e)
            {
                return Result.Fail(e.Message);
            }
        }


        public Result<AccountDto> BlockUser(AccountDto account)
        {
            if (!account.IsActive)
            {
                return account;
            }

            account.IsActive = false;
            var updateResult = Update(account);

            if (updateResult.IsSuccess)
            {
                return Result.Ok(account);
            }

            return Result.Fail(updateResult.Errors);
        }


        public UserProfileDto GetPersonByUserId(long userId)
        {
            var person = _personRepository.GetByUserId(userId);
            if (person == null)
                throw new Exception("Person not found.");

            return new UserProfileDto
            {
                Name = person.Name,
                Surname = person.Surname,
                Biography = person.Biography,
                Motto = person.Motto,
                ProfileImagePath = person.ProfileImagePath
            };
        }

        public void UpdatePerson(UserProfileDto dto, long userId)
        {
            var person = _personRepository.GetByUserId(userId);
            if (person == null)
                throw new Exception("Person not found.");

            if (dto.Name != null)
                person.Name = dto.Name;
            if (dto.Surname != null)
                person.Surname = dto.Surname;
            if (dto.Biography != null)
                person.Biography = dto.Biography;
            if (dto.Motto != null)
                person.Motto = dto.Motto;
            if (dto.ProfileImagePath != null)
                person.ProfileImagePath = dto.ProfileImagePath;

            _personRepository.Update(person);
        }
    }
}
