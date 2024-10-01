using Domain;
using Microsoft.EntityFrameworkCore;

namespace WebService.Services
{
    public class UserApiService : IUserApiService
    {

        private readonly DomainDbContext _context;

        public UserApiService(DomainDbContext context) 
        {
            _context = context;
        }

        public async Task AddUserAsync(CreateUserDTO userDto)
        {
            var groups = await _context.Groups
            .Where(g => userDto.GroupIds.Contains(g.Id))
            .ToListAsync();

            var user = new User
            {
                UserName = userDto.UserName,
                Groups = groups
            };

            _context.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return false;
            }

            _context.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Groups)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<int> GetUserCountAsync()
        {
            return await _context.Users.CountAsync();

        }

        public async Task<List<User>> GetUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<UpdateUserDTO> UpdateUserAsync(int id, UpdateUserDTO userDto)
        {
            var existingUser = await GetUserByIdAsync(id);
            if (existingUser == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found.");
            }

            existingUser.UserName = userDto.UserName;

            await UpdateUserGroupsAsync(existingUser, userDto.Groups);

            await _context.SaveChangesAsync();

            return MapToUpdateUserDTO(existingUser);
        }


        private async Task UpdateUserGroupsAsync(User existingUser, List<GroupDto> updatedGroups)
        {
            var currentGroupIds = existingUser.Groups.Select(g => g.Id).ToList();
            var updatedGroupIds = updatedGroups.Select(g => g.Id).ToList();

            var groupsToAdd = await _context.Groups
                .Where(g => updatedGroupIds.Contains(g.Id) && !currentGroupIds.Contains(g.Id))
                .ToListAsync();
            foreach (var group in groupsToAdd)
            {
                existingUser.Groups.Add(group);
            }

            var groupsToRemove = existingUser.Groups
                .Where(g => !updatedGroupIds.Contains(g.Id))
                .ToList();
            foreach (var group in groupsToRemove)
            {
                existingUser.Groups.Remove(group);
            }
        }

        private UpdateUserDTO MapToUpdateUserDTO(User user)
        {
            return new UpdateUserDTO
            {
                Id = user.Id,
                UserName = user.UserName,
                Groups = user.Groups
                .Select(g => new GroupDto { Id = g.Id, Name = g.Name }).ToList()
            };
        }

    }
}
