using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api_pendejos.DTO;
using Api_pendejos.Models;
using System.Security.Claims;

namespace Api_pendejos.Controllers
{
    [ApiController]
    [Route("api/cart")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly AndButMar_MarketplaceContext _context;

        public CartController(AndButMar_MarketplaceContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            var claim = User.FindFirstValue("sub");
            return int.TryParse(claim, out var id) ? id : 0;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = GetUserId();

            var items = await _context.Cart
                .Where(c => c.UserId == userId)
                .Select(c => new
                {
                    Product = new ProductResponse(
                        c.Product.Id,
                        c.Product.Name,
                        c.Product.Price,
                        c.Product.Description,
                        c.Product.CategoryId,
                        c.Product.Category.Name,
                        c.Product.ImagePath,
                        c.Product.StockQuantity,
                        c.Product.ItemsPerPackage,
                        c.Product.IsAvailable,
                        c.Product.Reviews.Any() ? c.Product.Reviews.Average(r => r.Rating) : 0.0),
                    c.Quantity,
                    LineTotal = c.Product.Price * c.Quantity
                })
                .ToListAsync();

            var total = items.Sum(i => i.LineTotal);

            return Ok(new { items, total });
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CartAddRequest request)
        {
            var userId = GetUserId();

            if (request.Quantity <= 0)
                return BadRequest(new { message = "Количество должно быть больше нуля." });

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId);
            if (product == null)
                return NotFound(new { message = "Товар не найден." });

            if (!product.IsAvailable)
                return BadRequest(new { message = "Товар недоступен для покупки." });

            var item = await _context.Cart
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == request.ProductId);

            var newQuantity = (item?.Quantity ?? 0) + request.Quantity;

            if (newQuantity > product.StockQuantity)
                return BadRequest(new { message = $"На складе только {product.StockQuantity} шт." });

            if (item == null)
            {
                _context.Cart.Add(new Cart
                {
                    UserId = userId,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity
                });
            }
            else
            {
                item.Quantity = newQuantity;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Товар добавлен в корзину.", productId = request.ProductId, quantity = newQuantity });
        }

        [HttpPut("{productId:int}")]
        public async Task<IActionResult> UpdateQuantity(int productId, [FromBody] CartUpdateRequest request)
        {
            var userId = GetUserId();

            if (request.Quantity <= 0)
                return BadRequest(new { message = "Количество должно быть больше нуля." });

            var item = await _context.Cart
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

            if (item == null)
                return NotFound(new { message = "Этого товара нет в корзине." });

            if (request.Quantity > item.Product.StockQuantity)
                return BadRequest(new { message = $"На складе только {item.Product.StockQuantity} шт." });

            item.Quantity = request.Quantity;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Количество обновлено.", productId, quantity = item.Quantity });
        }

        [HttpDelete("{productId:int}")]
        public async Task<IActionResult> Remove(int productId)
        {
            var userId = GetUserId();

            var item = await _context.Cart
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

            if (item == null)
                return NotFound(new { message = "Этого товара нет в корзине." });

            _context.Cart.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Товар удалён из корзины." });
        }
    }
}