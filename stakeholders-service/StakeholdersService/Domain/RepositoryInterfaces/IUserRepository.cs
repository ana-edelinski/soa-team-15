namespace StakeholdersService.Domain.RepositoryInterfaces
{
    public interface IUserRepository
    {
        bool Exists(string username);
        User Create(User user);
        long GetPersonId(long userId);
        User? GetById(long userId);
        bool IsAuthor(long userId);
    }
}
