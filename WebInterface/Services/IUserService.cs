using Domain;

namespace WebInterface.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> GetUserByIdAsync(int id);
        Task CreateUserAsync(CreateUserDTO dto);
        Task UpdateUserAsync(UpdateUserDTO dto);
        Task DeleteUserAsync(int id);
        Task<List<Group>> GetAllGroupsAsync();
    }
}
