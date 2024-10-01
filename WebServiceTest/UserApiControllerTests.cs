using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebService.Controllers;
using WebService.Services;

namespace WebServiceTest
{
    public class UserApiControllerTests
    {

        private DomainDbContext GetInMemoryDbContext()
        {

            var options = new DbContextOptionsBuilder<DomainDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new DomainDbContext(options);
        }

        [Fact]
        public async Task GetUsers_ReturnsOkWithUsers_WhenUsersExist()
        {
            var context = GetInMemoryDbContext();

            var user1 = new User { Id = 1, UserName = "TestUser" };
            var user2 = new User { Id = 2, UserName = "TestUser2" };

            context.Users.Add(user1);
            context.Users.Add(user2);
            await context.SaveChangesAsync();
            
            var controller = new UserApiController(new UserApiService(context));

            var result = await controller.GetUsers();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnUsers = Assert.IsType<List<User>>(okResult.Value);
            Assert.Equal(2, returnUsers.Count());
        }

        [Fact]
        public async Task GetUsers_ReturnsNotFound_WhenNoUsersExist()
        {
            var context = GetInMemoryDbContext();

            var controller = new UserApiController(new UserApiService(context));
            var result = await controller.GetUsers();

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetUser_ReturnOkWithUser()
        {
            var context = GetInMemoryDbContext();

            context.Users.Add(new User { Id = 1, UserName = "Test" });
            await context.SaveChangesAsync();

            var controller = new UserApiController(new UserApiService(context));

            var result = await controller.GetUser(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnUser = Assert.IsType<UpdateUserDTO>(okResult.Value);
        }

        [Fact]
        public async Task GetUser_ReturnNotFoundWithoutUser()
        {
            var context = GetInMemoryDbContext();

            var controller = new UserApiController(new UserApiService(context));
            var result = await controller.GetUser(1); 
            
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task AddUser_ReturnsOk_WhenUserIsAdded()
        {
            var context = GetInMemoryDbContext();

            var permission = new Permission { Id = 1, Name = "perm", Description = "test perm"};
            var group1 = new Group { Id = 1, Name = "Group 1",
                Description = "Test", Permissions = new List<Permission> { permission}
            };

            context.Permissions.Add(permission);
            context.Groups.Add(group1);
            await context.SaveChangesAsync();

            var newUser = new CreateUserDTO
            {
                UserName = "NewTestUser",
                GroupIds = new List<int> {1} 
            };

            var controller = new UserApiController(new UserApiService(context));

            var result = await controller.AddUser(newUser);

            var okResult = Assert.IsType<OkResult>(result);

            var addedUser = await context.Users.Include(u => u.Groups).FirstOrDefaultAsync(u => u.UserName == "NewTestUser");
            Assert.NotNull(addedUser);
            Assert.Equal("NewTestUser", addedUser.UserName);
            Assert.Equal(1, addedUser.Groups.Count); // Ensure the user is associated with both groups
            Assert.Contains(addedUser.Groups, g => g.Id == 1);
        }

        [Fact]
        public async Task DeleteUser_ReturnsOk_WhenUserExists()
        {
            var context = GetInMemoryDbContext();

            var user = new User { Id = 1, UserName = "TestUser" };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var controller = new UserApiController(new UserApiService(context));

            var result = await controller.DeleteUser(1);

            Assert.IsType<OkResult>(result);

            var deletedUser = await context.Users.FindAsync(1);
            Assert.Null(deletedUser);
        }

        [Fact]
        public async Task DeleteUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            var context = GetInMemoryDbContext();

            var controller = new UserApiController(new UserApiService(context));

            var result = await controller.DeleteUser(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateUser_ReturnsOk_WhenUserIsUpdated()
        {
            var context = GetInMemoryDbContext();

            var group1 = new Group { Id = 1, Name = "Group1", Description = "123" };
            var group2 = new Group { Id = 2, Name = "Group2", Description = "321" };
            context.Groups.AddRange(group1, group2);

            var user = new User { Id = 1, UserName = "TestUser", Groups = new List<Group> { group1 } };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var controller = new UserApiController(new UserApiService(context));

            var updatedUserDto = new UpdateUserDTO
            {
                Id = 1,
                UserName = "UpdatedUser",
                Groups = new List<GroupDto> { new GroupDto { Id = 2, Name = "Group2", Description = "321"} }
            };

            var result = await controller.UpdateUser(1, updatedUserDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnUser = Assert.IsType<UpdateUserDTO>(okResult.Value);

            // Verify user was updated
            Assert.Equal("UpdatedUser", returnUser.UserName);
            Assert.Single(returnUser.Groups);
            Assert.Equal(2, returnUser.Groups[0].Id);  // Ensure the correct group is associated

            // Check database for changes
            var updatedUser = await context.Users.Include(u => u.Groups).FirstOrDefaultAsync(u => u.Id == 1);
            Assert.Equal("UpdatedUser", updatedUser.UserName);
            Assert.Single(updatedUser.Groups);
            Assert.Equal(2, updatedUser.Groups.First().Id);  // Verify group was updated in the database
        }

        [Fact]
        public async Task UpdateUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            var context = GetInMemoryDbContext();
            var controller = new UserApiController(new UserApiService(context));

            var updatedUserDto = new UpdateUserDTO
            {
                Id = 1,
                UserName = "NonExistentUser",
                Groups = new List<GroupDto>()  // Empty group list
            };

            var result = await controller.UpdateUser(1, updatedUserDto);  // Non-existent user ID
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateUser_UpdatesUserGroups()
        {
            var context = GetInMemoryDbContext();

            var group1 = new Group { Id = 1, Name = "Group1", Description = "123"};
            var group2 = new Group { Id = 2, Name = "Group2", Description = "321"};
            var group3 = new Group { Id = 3, Name = "Group3", Description = "test"};
            context.Groups.AddRange(group1, group2, group3);

            var user = new User { Id = 1, UserName = "TestUser", Groups = new List<Group> { group1, group2 } };
            context.Users.Add(user);
            context.SaveChanges();

            var controller = new UserApiController(new UserApiService(context));

            var updatedUserDto = new UpdateUserDTO
            {
                Id = 1,
                UserName = "UpdatedUser",
                Groups = new List<GroupDto> { new GroupDto { Id = 3, Name = "Group3", Description = "test"}}  // Replace with Group3
            };

            var result = await controller.UpdateUser(1, updatedUserDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnUser = Assert.IsType<UpdateUserDTO>(okResult.Value);
            Assert.Equal("UpdatedUser", returnUser.UserName);
            Assert.Single(returnUser.Groups);
            Assert.Equal(3, returnUser.Groups[0].Id);  // Ensure Group3 is associated

            // Check database for changes
            var updatedUser = await context.Users.Include(u => u.Groups).FirstOrDefaultAsync(u => u.Id == 1);
            Assert.Single(updatedUser.Groups);
            Assert.Equal(3, updatedUser.Groups.First().Id);  // Verify Group3 in the database
        }

        [Fact]
        public async Task UserCount_ReturnsCorrectCount_WhenUsersExist()
        {
            var context = GetInMemoryDbContext();

            context.Users.Add(new User { Id = 1, UserName = "TestUser1" });
            context.Users.Add(new User { Id = 2, UserName = "TestUser2" });
            context.SaveChanges();

            var controller = new UserApiController(new UserApiService(context));

            var result = await controller.UserCount();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var count = Assert.IsType<int>(okResult.Value);
            Assert.Equal(2, count);
        }

        [Fact]
        public async Task UserCount_ReturnsZero_WhenNoUsersExist()
        {
            var context = GetInMemoryDbContext();
            var controller = new UserApiController(new UserApiService(context));

            var result = await controller.UserCount();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var count = Assert.IsType<int>(okResult.Value);
            Assert.Equal(0, count);  // No users should exist
        }

    }
}