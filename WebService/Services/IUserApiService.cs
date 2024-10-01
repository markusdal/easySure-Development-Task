using Domain;

namespace WebService.Services
{
    public interface IUserApiService
    {
        Task<List<User>> GetUsersAsync();
        Task<User> GetUserByIdAsync(int id);
        Task AddUserAsync(CreateUserDTO userDto);
        Task<UpdateUserDTO> UpdateUserAsync(int id, UpdateUserDTO userDto);
        Task<bool> DeleteUserAsync(int id);
        Task<int> GetUserCountAsync();
    }
}
