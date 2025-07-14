using System;

namespace StakeholdersService.Domain.RepositoryInterfaces
{
    public interface IPersonRepository
    {
        Person Create(Person person);
        Person? GetByUserId(long id);
        PagedResult<Person> GetPaged(int page, int pageSize);
    }
}
