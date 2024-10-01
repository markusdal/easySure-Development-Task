using Domain;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebInterface.Controllers;
using WebInterface.Services;
using WebInterface.ViewModels;

namespace WebInterfaceTest
{
    public class UserControllerTests
    {

        [Fact]
        public async Task Index_ReturnsViewWithUsers()
        {
            var mockUserService = new Mock<IUserService>();
            var users = new List<User>
        {
            new User { Id = 1, UserName = "TestUser1" },
            new User { Id = 2, UserName = "TestUser2" }
        };
            mockUserService.Setup(service => service.GetAllUsersAsync()).ReturnsAsync(users);

            var controller = new UserController(mockUserService.Object);

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<User>>(viewResult.Model);
            Assert.Equal(2, model.Count);  // Verify that 2 users are returned
            Assert.Equal("TestUser1", model[0].UserName);
        }

        [Fact]
        public async Task Index_ReturnsViewWithEmptyListWhenNoUsers()
        {
            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(service => service.GetAllUsersAsync()).ReturnsAsync((List<User>)null);

            var controller = new UserController(mockUserService.Object);

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<User>>(viewResult.Model);
            Assert.Empty(model);
        }

        [Fact]
        public async Task CreateUser_ReturnsViewWithAvailableGroups()
        {
            var mockUserService = new Mock<IUserService>();

            var groups = new List<Group> {
                new Group { Id = 1, Name = "Group1" },
                new Group { Id = 2, Name = "Group2" }
            };
            mockUserService.Setup(service => service.GetAllGroupsAsync()).ReturnsAsync(groups);

            var controller = new UserController(mockUserService.Object);

            var result = await controller.CreateUser();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<CreateUserViewModel>(viewResult.Model);
            Assert.Equal(2, model.AvailableGroups.Count);
            Assert.Empty(model.SelectedGroupIds);
        }

        [Fact]
        public async Task Edit_ReturnsViewWithUserAndGroups_WhenUserExists()
        {
            var mockUserService = new Mock<IUserService>();

            var user = new User
            {
                Id = 1,
                UserName = "TestUser",
                Groups = new List<Group> {
                    new Group { Id = 1, Name = "Group1" },
                    new Group { Id = 2, Name = "Group2" }
                }
            };
            mockUserService.Setup(service => service.GetUserByIdAsync(1)).ReturnsAsync(user);

            var allGroups = new List<Group> {
                new Group { Id = 1, Name = "Group1" },
                new Group { Id = 2, Name = "Group2" },
                new Group { Id = 3, Name = "Group3" }
            };
            mockUserService.Setup(service => service.GetAllGroupsAsync()).ReturnsAsync(allGroups);

            var controller = new UserController(mockUserService.Object);

            var result = await controller.Edit(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<CreateUserViewModel>(viewResult.Model);

            Assert.Equal(1, model.UserId);
            Assert.Equal("TestUser", model.UserName);
            Assert.Equal(3, model.AvailableGroups.Count);
            Assert.Equal(2, model.SelectedGroupIds.Count);
        }

        [Fact]
        public async Task Edit_ReturnsNotFound_WhenUserDoesNotExist()
        {
            var mockUserService = new Mock<IUserService>();

            mockUserService.Setup(service => service.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync((User)null);

            var controller = new UserController(mockUserService.Object);

            var result = await controller.Edit(1);
            Assert.IsType<NotFoundResult>(result);
        }
    }
}