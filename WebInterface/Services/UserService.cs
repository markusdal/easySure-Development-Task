using Domain;
using System.Text.Json;

namespace WebInterface.Services
{
    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;

        public UserService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("UserApi");
        }

        public async Task CreateUserAsync(CreateUserDTO userDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/userapi", userDto);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteUserAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/userapi/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            var response = await _httpClient.GetAsync("api/userapi");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<User>>();
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/userapi/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<User>();
        }

        public async Task UpdateUserAsync(UpdateUserDTO userDto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/userapi/{userDto.Id}", userDto);
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<Group>> GetAllGroupsAsync()
        {
            var response = await _httpClient.GetAsync("api/groupapi");
            response.EnsureSuccessStatusCode();
            var groupsJson = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<List<Group>>(groupsJson, options);
        }
    }
}
