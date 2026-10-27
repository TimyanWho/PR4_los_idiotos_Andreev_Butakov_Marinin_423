using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api_pendejos.DTO;
using Api_pendejos.Models;

namespace Api_pendejos.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly AndButMar_MarketplaceContext _context;

        public CategoriesController(AndButMar_MarketplaceContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    ProductsCount = c.Products.Count()
                })
                .ToListAsync();

            return Ok(categories);
        }

        [HttpPost]
        [Authorize(Roles = "Администратор")]
        public async Task<IActionResult> Create([FromBody] CategoryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Укажите название категории." });

            var exists = await _context.Categories.AnyAsync(c => c.Name == request.Name);
            if (exists)
                return Conflict(new { message = "Такая категория уже существует." });

            var category = new Categories { Name = request.Name };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Created($"/api/categories/{category.Id}", new { category.Id, category.Name });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Администратор")]
        public async Task<IActionResult> Rename(int id, [FromBody] CategoryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Укажите название категории." });

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
                return NotFound(new { message = "Категория не найдена." });

            var duplicate = await _context.Categories
                .AnyAsync(c => c.Name == request.Name && c.Id != id);
            if (duplicate)
                return Conflict(new { message = "Категория с таким названием уже существует." });

            category.Name = request.Name;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Категория обновлена." });
        }
    }
}