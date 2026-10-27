using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api_pendejos.DTO;
using Api_pendejos.Models;
using System.Security.Claims;

namespace Api_pendejos.Controllers
{
    [ApiController]
    [Route("api/favorites")]
    [Authorize]
    public class FavoritesController : ControllerBase
    {
        private readonly AndButMar_MarketplaceContext _context;

        public FavoritesController(AndButMar_MarketplaceContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            var claim = User.FindFirstValue("sub");
            return int.TryParse(claim, out var id) ? id : 0;
        }

        [HttpGet]
        public async Task<IActionResult> GetFavorites()
        {
            var userId = GetUserId();

            var favorites = await _context.Favorites
                .Where(f => f.UserId == userId)
                .Select(f => new ProductResponse(
                    f.Product.Id,
                    f.Product.Name,
                    f.Product.Price,
                    f.Product.Description,
                    f.Product.CategoryId,
                    f.Product.Category.Name,
                    f.Product.ImagePath,
                    f.Product.StockQuantity,
                    f.Product.ItemsPerPackage,
                    f.Product.IsAvailable,
                    f.Product.Reviews.Any() ? f.Product.Reviews.Average(r => r.Rating) : 0.0))
                .ToListAsync();

            return Ok(favorites);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] FavoriteAddRequest request)
        {
            var userId = GetUserId();

            var productExists = await _context.Products.AnyAsync(p => p.Id == request.ProductId);
            if (!productExists)
                return NotFound(new { message = "Товар не найден." });

            var already = await _context.Favorites
                .AnyAsync(f => f.UserId == userId && f.ProductId == request.ProductId);
            if (already)
                return Ok(new { message = "Товар уже в избранном." });

            _context.Favorites.Add(new Favorites
            {
                UserId = userId,
                ProductId = request.ProductId
            });

            await _context.SaveChangesAsync();

            return Ok(new { message = "Товар добавлен в избранное." });
        }

        [HttpDelete("{productId:int}")]
        public async Task<IActionResult> Remove(int productId)
        {
            var userId = GetUserId();

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);

            if (favorite == null)
                return NotFound(new { message = "Этого товара нет в избранном." });

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Товар удалён из избранного." });
        }

        [HttpPost("{productId:int}/to-cart")]
        public async Task<IActionResult> MoveToCart(int productId)
        {
            var userId = GetUserId();

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);

            if (favorite == null)
                return NotFound(new { message = "Этого товара нет в избранном." });

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null || !product.IsAvailable || product.StockQuantity < 1)
                return BadRequest(new { message = "Товар недоступен для покупки." });

            var cartItem = await _context.Cart
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

            if (cartItem == null)
            {
                _context.Cart.Add(new Cart
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = 1
                });
            }
            else
            {
                cartItem.Quantity += 1;
            }

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Товар перенесён в корзину." });
        }
    }
}