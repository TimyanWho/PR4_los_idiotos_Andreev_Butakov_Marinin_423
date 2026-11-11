using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api_pendejos.DTO;
using Api_pendejos.Models;
using System.Security.Claims;

namespace Api_pendejos.Controllers
{
    [ApiController]
    [Route("api/reviews")]
    public class ReviewsController : ControllerBase
    {
        private readonly AndButMar_MarketplaceContext _context;

        public ReviewsController(AndButMar_MarketplaceContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            var claim = User.FindFirstValue("sub");
            return int.TryParse(claim, out var id) ? id : 0;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetByProduct([FromQuery] int productId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.Id,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt,
                    Author = r.User.Login
                })
                .ToListAsync();

            var average = reviews.Count == 0
                ? 0.0
                : Math.Round(reviews.Average(r => (double)r.Rating), 1);

            return Ok(new { productId, average, reviews });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] ReviewCreateRequest request)
        {
            var userId = GetUserId();

            if (request.Rating < 1 || request.Rating > 5)
                return BadRequest(new { message = "Оценка должна быть от 1 до 5." });

            var productExists = await _context.Products.AnyAsync(p => p.Id == request.ProductId);
            if (!productExists)
                return NotFound(new { message = "Товар не найден." });

            var alreadyReviewed = await _context.Reviews
                .AnyAsync(r => r.UserId == userId && r.ProductId == request.ProductId);
            if (alreadyReviewed)
                return Conflict(new { message = "Вы уже оставляли отзыв на этот товар." });

            var review = new Reviews
            {
                UserId = userId,
                ProductId = request.ProductId,
                Rating = request.Rating,
                Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment,
                CreatedAt = DateTime.Now
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return Created($"/api/reviews?productId={request.ProductId}", new { review.Id, review.Rating });
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] ReviewUpdateRequest request)
        {
            var userId = GetUserId();

            if (request.Rating < 1 || request.Rating > 5)
                return BadRequest(new { message = "Оценка должна быть от 1 до 5." });

            var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == id);
            if (review == null)
                return NotFound(new { message = "Отзыв не найден." });

            if (review.UserId != userId)
                return Forbid();

            review.Rating = request.Rating;
            review.Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Отзыв обновлён." });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Администратор")]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == id);
            if (review == null)
                return NotFound(new { message = "Отзыв не найден." });

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Отзыв удалён." });
        }
    }
}