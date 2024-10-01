using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebService.Controllers;

namespace WebServiceTest
{
    public class GroupApiControllerTests
    {

        private DomainDbContext GetInMemoryDbContext()
        {

            var options = new DbContextOptionsBuilder<DomainDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new DomainDbContext(options);
        }

        [Fact]
        public async Task GetGroups_ReturnsOkWithGroups_WhenGroupsExist()
        {
            var context = GetInMemoryDbContext();

            context.Groups.Add(new Group { Id = 1, Name = "Group1", Description = "123"});
            context.Groups.Add(new Group { Id = 2, Name = "Group2", Description = "321"});
            context.SaveChanges();

            var controller = new GroupApiController(context);

            var result = await controller.GetGroups();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnGroups = Assert.IsType<List<Group>>(okResult.Value);
            Assert.Equal(2, returnGroups.Count);  // We added 2 groups
        }

        [Fact]
        public async Task GetGroups_ReturnsEmptyList_WhenNoGroupsExist()
        {
            var context = GetInMemoryDbContext();

            var controller = new GroupApiController(context);

            var result = await controller.GetGroups();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnGroups = Assert.IsType<List<Group>>(okResult.Value);
            Assert.Empty(returnGroups);  // Expect the list to be empty
        }

        [Fact]
        public async Task GetGroupById_ReturnsOkWithGroup_WhenGroupExists()
        {
            var context = GetInMemoryDbContext();

            var group = new Group { Id = 1, Name = "Test Group", Description = "123"};
            context.Groups.Add(group);
            context.SaveChanges();  // Save the changes

            var controller = new GroupApiController(context);

            var result = await controller.GetGroupById(1);  // Pass the ID of the added group

            var okResult = Assert.IsType<OkObjectResult>(result);  // Ensure result is 200 OK
            var returnGroup = Assert.IsType<Group>(okResult.Value);  // Ensure the result contains a group
            Assert.Equal(1, returnGroup.Id);  // Verify the group's ID matches
            Assert.Equal("Test Group", returnGroup.Name);  // Verify the group's Name matches
        }

        [Fact]
        public async Task GetGroupById_ReturnsNotFound_WhenGroupDoesNotExist()
        {
            var context = GetInMemoryDbContext();

            var controller = new GroupApiController(context);

            var result = await controller.GetGroupById(999);  // Pass an ID that does not exist

            Assert.IsType<NotFoundResult>(result);  // Expect a 404 NotFound result
        }

        [Fact]
        public async Task UserCountInGroup_ReturnsOkWithUserCount_WhenGroupExists()
        {
            var context = GetInMemoryDbContext();

            var group = new Group { Id = 1, Name = "Test Group"
                , Description = "123", Users = new List<User>()};
            var user1 = new User { Id = 1, UserName = "TestUser1" };
            var user2 = new User { Id = 2, UserName = "TestUser2" };

            group.Users.Add(user1);
            group.Users.Add(user2);

            context.Groups.Add(group);
            context.SaveChanges();  // Save the group and users

            var controller = new GroupApiController(context);

            var result = await controller.UserCountInGroup(1);  // Pass the ID of the added group

            var okResult = Assert.IsType<OkObjectResult>(result);  // Ensure result is 200 OK
            var userCount = Assert.IsType<int>(okResult.Value);  // Ensure the result contains an integer
            Assert.Equal(2, userCount);  // Verify the count matches the number of users in the group
        }

        [Fact]
        public async Task UserCountInGroup_ReturnsNotFound_WhenGroupDoesNotExist()
        {
            var context = GetInMemoryDbContext();

            var controller = new GroupApiController(context);

            var result = await controller.UserCountInGroup(999);  // Pass an ID that does not exist
            Assert.IsType<NotFoundResult>(result);  // Expect a 404 NotFound result
        }

    }
}
