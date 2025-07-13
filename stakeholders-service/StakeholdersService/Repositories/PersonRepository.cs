using StakeholdersService.Database;
using StakeholdersService.Domain.RepositoryInterfaces;
using StakeholdersService.Domain;

namespace StakeholdersService.Repositories
{
    public class PersonRepository : IPersonRepository
    {
        private readonly StakeholdersContext _dbContext;

        public PersonRepository(StakeholdersContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Person Create(Person person)
        {
            _dbContext.People.Add(person);
            _dbContext.SaveChanges();
            return person;
        }

        public Person? GetByUserId(long id)
        {
            return _dbContext.People.FirstOrDefault(p => p.UserId == id);
        }
    }
}
