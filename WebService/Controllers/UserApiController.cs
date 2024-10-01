using Domain;
using Microsoft.AspNetCore.Mvc;
using WebService.Services;

namespace WebService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserApiController : Controller
    {
        private readonly IUserApiService _userApiService;

        public UserApiController(IUserApiService userApiService)
        {
            _userApiService = userApiService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userApiService.GetUsersAsync();
            if (users.Count == 0)
            {
                return NotFound();
            }

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _userApiService.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var userDto = new UpdateUserDTO
            {
                Id = user.Id,
                UserName = user.UserName,
                Groups = user.Groups.Select(g => new GroupDto
                {
                    Id = g.Id,
                    Name = g.Name
                }).ToList()
            };

            return Ok(userDto);
        }

        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] CreateUserDTO userDto)
        {
            await _userApiService.AddUserAsync(userDto);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDTO userDto)
        {
            try
            {
                var updatedUser = await _userApiService.UpdateUserAsync(id, userDto);
                return Ok(updatedUser);

            }catch(KeyNotFoundException ex)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var deleted = await _userApiService.DeleteUserAsync(id);

            if (!deleted)
            {
                return NotFound();
            }

            return Ok();
        }

        [HttpGet("count")]
        public async Task<IActionResult> UserCount()
        {
            var count = await _userApiService.GetUserCountAsync();
            return Ok(count);
        }
    }
}
