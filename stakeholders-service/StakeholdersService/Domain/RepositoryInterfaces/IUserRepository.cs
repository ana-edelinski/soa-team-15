namespace StakeholdersService.Domain.RepositoryInterfaces
{
    public interface IUserRepository
    {
        bool Exists(string username);
        User? GetActiveByName(string username);
        User Create(User user);
        long GetPersonId(long userId);
        List<User> GetActiveUsers();
        User? GetById(long userId);
        bool IsAuthor(long userId);
    }
}
