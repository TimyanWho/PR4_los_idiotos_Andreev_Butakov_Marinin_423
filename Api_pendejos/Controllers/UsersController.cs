using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api_pendejos.Models;
using System.Security.Claims;

namespace Api_pendejos.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly AndButMar_MarketplaceContext _context;

        public UsersController(AndButMar_MarketplaceContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            var claim = User.FindFirstValue("sub");
            return int.TryParse(claim, out var id) ? id : 0;
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == GetUserId());

            if (user == null)
                return NotFound(new { message = "Пользователь не найден." });

            return Ok(new { user.Id, user.Login, Role = user.Role.Name });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound(new { message = "Пользователь не найден." });

            if (user.Id != GetUserId() && !User.IsInRole("Администратор"))
                return Forbid();

            return Ok(new { user.Id, user.Login, Role = user.Role.Name, user.CreatedAt });
        }
    }
}