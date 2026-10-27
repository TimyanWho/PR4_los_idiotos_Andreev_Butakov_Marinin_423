using Api_pendejos;
using Api_pendejos.DTO;
using Api_pendejos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api_pendejos.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly AndButMar_MarketplaceContext _context;

        public ProductsController(AndButMar_MarketplaceContext context)
        {
            _context = context;
        }

        // 1. ПОЛУЧЕНИЕ КАТАЛОГА
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] int? categoryId = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .AsQueryable();

            // Если запрашивает НЕ администратор, показываем только доступные товары на складе
            if (!User.IsInRole("Администратор"))
            {
                query = query.Where(p => p.IsAvailable == true && p.StockQuantity > 0);
            }

            // Фильтр: Поиск по названию
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Name.Contains(search));
            }

            // Фильтр: По категории
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // Фильтр: По минимальной цене
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            // Фильтр: По максимальной цене
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            var products = await query.Select(p => new ProductResponse(
                p.Id,
                p.Name,
                p.Price,
                p.Description,
                p.CategoryId,
                p.Category.Name,
                p.ImagePath,
                p.StockQuantity,
                p.ItemsPerPackage,
                p.IsAvailable,
                p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0.0
            )).ToListAsync();

            return Ok(products);
        }

        // 2. ДЕТАЛИ ТОВАРА (Доступно гостю)
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var p = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (p == null) return NotFound(new { message = "Товар не найден" });

            var response = new ProductResponse(
                p.Id,
                p.Name,
                p.Price,
                p.Description,
                p.CategoryId,
                p.Category.Name,
                p.ImagePath,
                p.StockQuantity,
                p.ItemsPerPackage,
                p.IsAvailable,
                p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0.0
            );

            return Ok(response);
        }

        // 3. ДОБАВЛЕНИЕ ТОВАРА (Только Администратор)
        [HttpPost]
        [Authorize(Roles = "Администратор")]
        public async Task<IActionResult> Create([FromBody] SaveProductRequest request)
        {
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);
            if (!categoryExists) return BadRequest(new { message = "Указанная категория не существует" });

            var product = new Products
            {
                Name = request.Name,
                Price = request.Price,
                Description = request.Description,
                CategoryId = request.CategoryId,
                ImagePath = request.ImagePath,
                StockQuantity = request.StockQuantity,
                ItemsPerPackage = request.ItemsPerPackage,
                IsAvailable = request.IsAvailable,
                CreatedAt = DateTime.Now
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        // 4. РЕДАКТИРОВАНИЕ ТОВАРА (Только Администратор)
        [HttpPut("{id}")]
        [Authorize(Roles = "Администратор")]
        public async Task<IActionResult> Update(int id, [FromBody] SaveProductRequest request)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound(new { message = "Товар не найден" });

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);
            if (!categoryExists) return BadRequest(new { message = "Указанная категория не существует" });

            product.Name = request.Name;
            product.Price = request.Price;
            product.Description = request.Description;
            product.CategoryId = request.CategoryId;
            product.ImagePath = request.ImagePath;
            product.StockQuantity = request.StockQuantity;
            product.ItemsPerPackage = request.ItemsPerPackage;
            product.IsAvailable = request.IsAvailable;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Товар успешно обновлен" });
        }

        // 5. УДАЛЕНИЕ ТОВАРА (Только Администратор)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Администратор")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound(new { message = "Товар не найден" });

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Товар удален" });
        }
    }
}
