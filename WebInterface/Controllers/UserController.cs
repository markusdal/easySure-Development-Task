using Domain;
using Microsoft.AspNetCore.Mvc;
using WebInterface.Services;
using WebInterface.ViewModels;

namespace WebInterface.Controllers
{
    public class UserController : Controller
    {

        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users ?? new List<User>());
        }

        [HttpGet]
        public async Task<IActionResult> CreateUser()
        {
            var groups = await _userService.GetAllGroupsAsync();
            var model = new CreateUserViewModel
            {
                AvailableGroups = groups,
                SelectedGroupIds = new List<int>()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            var createUserDto = new CreateUserDTO
            {
                UserName = model.UserName,
                GroupIds = model.SelectedGroupIds
            };

            await _userService.CreateUserAsync(createUserDto);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            var groups = await _userService.GetAllGroupsAsync();

            if (user == null)
            {
                return NotFound();
            }

            var model = new CreateUserViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                AvailableGroups = groups,
                SelectedGroupIds = user.Groups.Select(g => g.Id).ToList()
            };

            return View("CreateUser", model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CreateUserViewModel model)
        {

            if (!ModelState.IsValid)
            {
                Console.WriteLine(ModelState.First());
                model.AvailableGroups = await _userService.GetAllGroupsAsync();
                return View("CreateUser", model);
            }

            var userDto = new UpdateUserDTO
            {
                Id = (int)model.UserId,
                UserName = model.UserName,
                Groups = model.SelectedGroupIds.Select(id => new GroupDto { Id = id }).ToList() // Adjust as needed
            };

            await _userService.UpdateUserAsync(userDto);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            await _userService.DeleteUserAsync(id);
            return RedirectToAction("Index");
        }

        /*
        public async Task<IActionResult> GetUserCount()
        {
            var response = await _httpClient.GetAsync("api/userapi/count");

            if(response.IsSuccessStatusCode)
            {
                var count = response.Content.ReadAsStringAsync();
                return View(count);
            }

            return View();
        }
        */
    }
}
