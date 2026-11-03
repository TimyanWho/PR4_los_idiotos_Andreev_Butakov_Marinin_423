using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api_pendejos.DTO;
using Api_pendejos.Models;

namespace Api_pendejos.Controllers
{
    [ApiController]
    [Route("api/pickup-points")]
    public class PickupPointsController : ControllerBase
    {
        private readonly AndButMar_MarketplaceContext _context;

        public PickupPointsController(AndButMar_MarketplaceContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var points = await _context.PickupPoints
                .OrderBy(p => p.Address)
                .Select(p => new { p.Id, p.Name, p.Address })
                .ToListAsync();

            return Ok(points);
        }

        [HttpPost]
        [Authorize(Roles = "Администратор")]
        public async Task<IActionResult> Create([FromBody] PickupPointRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Address))
                return BadRequest(new { message = "Укажите адрес ПВЗ." });

            var exists = await _context.PickupPoints.AnyAsync(p => p.Address == request.Address);
            if (exists)
                return Conflict(new { message = "ПВЗ с таким адресом уже существует." });

            var point = new PickupPoints
            {
                Address = request.Address,
                Name = string.IsNullOrWhiteSpace(request.Name) ? request.Address : request.Name
            };

            _context.PickupPoints.Add(point);
            await _context.SaveChangesAsync();

            return Created($"/api/pickup-points/{point.Id}", new { point.Id, point.Name, point.Address });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Администратор")]
        public async Task<IActionResult> Update(int id, [FromBody] PickupPointRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Address))
                return BadRequest(new { message = "Укажите адрес ПВЗ." });

            var point = await _context.PickupPoints.FirstOrDefaultAsync(p => p.Id == id);
            if (point == null)
                return NotFound(new { message = "ПВЗ не найден." });

            var duplicate = await _context.PickupPoints
                .AnyAsync(p => p.Address == request.Address && p.Id != id);
            if (duplicate)
                return Conflict(new { message = "ПВЗ с таким адресом уже существует." });

            point.Address = request.Address;
            point.Name = string.IsNullOrWhiteSpace(request.Name) ? request.Address : request.Name;

            await _context.SaveChangesAsync();

            return Ok(new { message = "ПВЗ обновлён." });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Администратор")]
        public async Task<IActionResult> Delete(int id)
        {
            var point = await _context.PickupPoints.FirstOrDefaultAsync(p => p.Id == id);
            if (point == null)
                return NotFound(new { message = "ПВЗ не найден." });

            var hasOrders = await _context.Orders.AnyAsync(o => o.PickupPointId == id);
            if (hasOrders)
                return Conflict(new { message = "Нельзя удалить ПВЗ, к нему привязаны заказы." });

            _context.PickupPoints.Remove(point);
            await _context.SaveChangesAsync();

            return Ok(new { message = "ПВЗ удалён." });
        }
    }
}