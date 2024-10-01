using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroupApiController : Controller
    {
        private readonly DomainDbContext _context;

        public GroupApiController(DomainDbContext context) 
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetGroups()
        {
            var groups = await _context.Groups.ToListAsync();
            return Ok(groups);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGroupById(int id)
        {
            var group = await _context.Groups.FindAsync(id);
            if (group == null)
            {
                return NotFound();
            }
            return Ok(group);
        }

        [HttpGet("{id}/usercount")]
        public async Task<IActionResult> UserCountInGroup(int id)
        {
            var group = await _context.Groups
                .Include(g => g.Users)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
            {
                return NotFound();
            }

            var userCount = group.Users.Count;
            return Ok(userCount);
        }
    }
}
