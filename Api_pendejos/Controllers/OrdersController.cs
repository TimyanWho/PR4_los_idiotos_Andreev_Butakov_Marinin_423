using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api_pendejos.DTO;
using Api_pendejos.Models;
using System.Security.Claims;

namespace Api_pendejos.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private const decimal DeliveryMinimum = 600m;

        private readonly AndButMar_MarketplaceContext _context;

        public OrdersController(AndButMar_MarketplaceContext context)
        {
            _context = context;
        }

        private sealed record DeliveryRule(
            string? ErrorMessage,
            string DeliveryType = "",
            string? DeliveryAddress = null,
            int? PickupPointId = null);

        private int GetUserId()
        {
            var claim = User.FindFirstValue("sub");
            return int.TryParse(claim, out var id) ? id : 0;
        }

        [HttpGet("statuses")]
        public async Task<IActionResult> GetStatuses()
        {
            var statuses = await _context.OrderStatuses
                .OrderBy(s => s.Id)
                .Select(s => new { s.Id, s.Name })
                .ToListAsync();

            return Ok(statuses);
        }

        [HttpGet]
        public async Task<IActionResult> GetMy([FromQuery] string? sort)
        {
            var userId = GetUserId();

            var query = _context.Orders.Where(o => o.UserId == userId);

            query = string.Equals(sort, "asc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(o => o.CreatedAt)
                : query.OrderByDescending(o => o.CreatedAt);

            var orders = await query
                .Select(o => new
                {
                    o.Id,
                    o.CreatedAt,
                    Status = o.Status.Name,
                    o.DeliveryType,
                    o.TotalPrice,
                    Positions = o.OrderItems.Count(),
                    TotalQuantity = o.OrderItems.Sum(i => i.Quantity)
                })
                .ToListAsync();

            return Ok(orders);
        }

        [HttpGet("all")]
        [Authorize(Roles = "Администратор")]
        public async Task<IActionResult> GetAll([FromQuery] int? userId, [FromQuery] int? statusId)
        {
            var query = _context.Orders.AsQueryable();

            if (userId.HasValue)
                query = query.Where(o => o.UserId == userId.Value);

            if (statusId.HasValue)
                query = query.Where(o => o.StatusId == statusId.Value);

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new
                {
                    o.Id,
                    o.CreatedAt,
                    Buyer = o.User.Login,
                    Status = o.Status.Name,
                    o.DeliveryType,
                    o.TotalPrice
                })
                .ToListAsync();

            return Ok(orders);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Status)
                .Include(o => o.PickupPoint)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound(new { message = "Заказ не найден." });

            if (!User.IsInRole("Администратор") && order.UserId != GetUserId())
                return Forbid();

            var items = await _context.OrderItems
                .Where(i => i.OrderId == id)
                .Select(i => new
                {
                    i.ProductId,
                    Product = i.Product.Name,
                    i.Quantity,
                    i.PriceAtPurchase,
                    LineTotal = i.PriceAtPurchase * i.Quantity
                })
                .ToListAsync();

            return Ok(new
            {
                order.Id,
                order.CreatedAt,
                Status = order.Status.Name,
                order.DeliveryType,
                DeliveryAddress = order.DeliveryAddress,
                PickupPoint = order.PickupPoint == null
                    ? null
                    : new { order.PickupPoint.Id, order.PickupPoint.Name, order.PickupPoint.Address },
                Buyer = order.User.Login,
                order.TotalPrice,
                Items = items
            });
        }

        [HttpPost("direct")]
        public async Task<IActionResult> CreateDirect([FromBody] DirectOrderRequest request)
        {
            var userId = GetUserId();

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId);
            if (product == null)
                return NotFound(new { message = "Товар не найден." });

            if (!product.IsAvailable)
                return BadRequest(new { message = "Товар недоступен для покупки." });

            if (request.Quantity <= 0)
                return BadRequest(new { message = "Количество должно быть больше нуля." });

            if (product.StockQuantity < request.Quantity)
                return BadRequest(new { message = $"На складе только {product.StockQuantity} шт." });

            var total = product.Price * request.Quantity;

            var delivery = await ValidateDeliveryAsync(request.DeliveryType, request.DeliveryAddress,
                request.PickupPointId, total);
            if (delivery.ErrorMessage != null)
                return BadRequest(new { message = delivery.ErrorMessage });

            var statusId = await _context.OrderStatuses
                .OrderBy(s => s.Id)
                .Select(s => (int?)s.Id)
                .FirstOrDefaultAsync() ?? 1;

            var order = new Orders
            {
                UserId = userId,
                CreatedAt = DateTime.Now,
                DeliveryType = delivery.DeliveryType,
                DeliveryAddress = delivery.DeliveryAddress,
                PickupPointId = delivery.PickupPointId,
                TotalPrice = total,
                StatusId = statusId
            };

            order.OrderItems.Add(new OrderItems
            {
                ProductId = product.Id,
                Quantity = request.Quantity,
                PriceAtPurchase = product.Price
            });

            product.StockQuantity -= request.Quantity;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return Created($"/api/orders/{order.Id}", new { order.Id, total = order.TotalPrice });
        }

        [HttpPost("from-cart")]
        public async Task<IActionResult> CreateFromCart([FromBody] CartOrderRequest request)
        {
            var userId = GetUserId();

            var cartItems = await _context.Cart
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .ToListAsync();

            if (cartItems.Count == 0)
                return BadRequest(new { message = "Корзина пуста." });

            var problems = cartItems
                .Where(c => !c.Product.IsAvailable || c.Product.StockQuantity < c.Quantity)
                .Select(c => new { c.ProductId, Product = c.Product.Name })
                .ToList();

            if (problems.Count > 0)
                return BadRequest(new
                {
                    message = "Часть товаров недоступна или закончилась на складе.",
                    products = problems
                });

            var total = cartItems.Sum(c => c.Product.Price * c.Quantity);

            var delivery = await ValidateDeliveryAsync(request.DeliveryType, request.DeliveryAddress,
                request.PickupPointId, total);
            if (delivery.ErrorMessage != null)
                return BadRequest(new { message = delivery.ErrorMessage });

            var statusId = await _context.OrderStatuses
                .OrderBy(s => s.Id)
                .Select(s => (int?)s.Id)
                .FirstOrDefaultAsync() ?? 1;

            var order = new Orders
            {
                UserId = userId,
                CreatedAt = DateTime.Now,
                DeliveryType = delivery.DeliveryType,
                DeliveryAddress = delivery.DeliveryAddress,
                PickupPointId = delivery.PickupPointId,
                TotalPrice = total,
                StatusId = statusId
            };

            foreach (var item in cartItems)
            {
                order.OrderItems.Add(new OrderItems
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    PriceAtPurchase = item.Product.Price
                });

                item.Product.StockQuantity -= item.Quantity;
            }

            _context.Cart.RemoveRange(cartItems);
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return Created($"/api/orders/{order.Id}", new { order.Id, total = order.TotalPrice });
        }

        [HttpPut("{id:int}/status")]
        [Authorize(Roles = "Администратор")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] OrderStatusRequest request)
        {
            var statusExists = await _context.OrderStatuses.AnyAsync(s => s.Id == request.StatusId);
            if (!statusExists)
                return BadRequest(new { message = "Такой статус не найден." });

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
                return NotFound(new { message = "Заказ не найден." });

            order.StatusId = request.StatusId;
            await _context.SaveChangesAsync();

            return Ok(new { order.Id, order.StatusId });
        }

        private async Task<DeliveryRule> ValidateDeliveryAsync(
            string? requestType,
            string? address,
            int? pickupPointId,
            decimal total)
        {
            if (string.IsNullOrWhiteSpace(requestType))
                return new DeliveryRule("Выберите способ получения.");

            if (requestType.Equals("Доставка", StringComparison.OrdinalIgnoreCase))
            {
                if (total < DeliveryMinimum)
                    return new DeliveryRule($"Доставка доступна только для заказов от {DeliveryMinimum} рублей.");

                if (string.IsNullOrWhiteSpace(address))
                    return new DeliveryRule("Укажите адрес доставки.");

                return new DeliveryRule(null, "Доставка", address, null);
            }

            if (requestType.Equals("Самовывоз", StringComparison.OrdinalIgnoreCase))
            {
                if (pickupPointId == null)
                    return new DeliveryRule("Выберите пункт выдачи.");

                var point = await _context.PickupPoints.FindAsync(pickupPointId.Value);
                if (point == null)
                    return new DeliveryRule("Указанный ПВЗ не найден.");

                return new DeliveryRule(null, "Самовывоз", null, point.Id);
            }

            return new DeliveryRule("Способ получения: 'Доставка' или 'Самовывоз'.");
        }
    }
}