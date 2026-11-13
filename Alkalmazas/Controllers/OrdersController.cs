using Alkalmazas.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Alkalmazas.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _db;

        public OrdersController(AppDbContext db)
        {
            _db = db;
        }

        // GET api/orders
        [HttpGet]
        public async Task<ActionResult<List<OrderResponse>>> GetAll()
        {
            var orders = await _db.Orders
                .Include(o => o.Lines)
                .OrderByDescending(o => o.CreatedAtUtc)
                .ThenByDescending(o => o.Id)
                .ToListAsync();

            return orders.Select(ToResponse).ToList();
        }

        // GET api/orders/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderResponse>> GetById(int id)
        {
            var order = await _db.Orders.Include(o => o.Lines).FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound("Nem található a rendelés.");
            }

            return ToResponse(order);
        }

        // POST api/orders
        // {"userId": 1, "orderDate": "2026-12-01", "location": "Budapest", "catGirlIds": [1, 2]}
        [HttpPost]
        public async Task<ActionResult<OrderResponse>> Create(OrderRequest request)
        {
            var error = InputValidation.CheckOrder(request.OrderDate, request.Location)
                ?? InputValidation.CheckContact(request.CustomerName, request.CustomerEmail, request.CustomerPhone);

            if (error != null)
            {
                return BadRequest(error);
            }

            if (!await _db.Users.AnyAsync(u => u.Id == request.UserId))
            {
                return BadRequest("Nincs ilyen felhasználó.");
            }

            var ids = request.CatGirlIds.Distinct().ToList();

            if (ids.Count == 0)
            {
                return BadRequest("Legalább egy cicálányt ki kell választani.");
            }

            var cats = await _db.CatGirls.Where(c => ids.Contains(c.Id)).ToListAsync();

            if (cats.Count != ids.Count)
            {
                return BadRequest("Valamelyik cicálány nem található.");
            }

            var order = new Order
            {
                UserId = request.UserId,
                OrderDate = request.OrderDate,
                Location = CleanLocation(request.Location),
                CustomerName = request.CustomerName.Trim(),
                CustomerEmail = request.CustomerEmail.Trim(),
                CustomerPhone = request.CustomerPhone.Trim(),
                CreatedAtUtc = DateTime.UtcNow
            };

            // a rendelés a rendeléskori adatokat őrzi meg
            foreach (var cat in cats)
            {
                order.Lines.Add(new OrderLine
                {
                    CatGirlId = cat.Id,
                    Name = cat.Name,
                    Age = cat.Age,
                    HeightCm = cat.HeightCm,
                    WeightKg = cat.WeightKg
                });
            }

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = order.Id }, ToResponse(order));
        }

        // PUT api/orders/5   (dátum és helyszín módosítása)
        [HttpPut("{id:int}")]
        public async Task<ActionResult<OrderResponse>> Update(int id, OrderUpdateRequest request)
        {
            var order = await _db.Orders.Include(o => o.Lines).FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound("Nem található a rendelés.");
            }

            var error = InputValidation.CheckOrder(request.OrderDate, request.Location);

            if (error != null)
            {
                return BadRequest(error);
            }

            order.OrderDate = request.OrderDate;
            order.Location = CleanLocation(request.Location);

            await _db.SaveChangesAsync();

            return ToResponse(order);
        }

        // DELETE api/orders/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _db.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound("Nem található a rendelés.");
            }

            _db.Orders.Remove(order);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // SAJÁT FÜGGVÉNY: GET api/orders/user/5   (egy felhasználó rendelései)
        [HttpGet("user/{userId:int}")]
        public async Task<ActionResult<List<OrderResponse>>> GetByUser(int userId)
        {
            if (!await _db.Users.AnyAsync(u => u.Id == userId))
            {
                return NotFound("Nem található a felhasználó.");
            }

            var orders = await _db.Orders
                .Include(o => o.Lines)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAtUtc)
                .ThenByDescending(o => o.Id)
                .ToListAsync();

            return orders.Select(ToResponse).ToList();
        }

        private static string CleanLocation(string? location) =>
            string.IsNullOrWhiteSpace(location) ? "Online" : location.Trim();

        private static OrderResponse ToResponse(Order order)
        {
            var lines = order.Lines
                .OrderBy(l => l.Id)
                .Select(l => new OrderLineResponse(
                    l.CatGirlId, l.Name, l.Age, l.HeightCm, l.WeightKg, l.Price, l.DurationHours))
                .ToList();

            return new OrderResponse(
                order.Id,
                order.UserId,
                order.OrderDate,
                order.Location,
                order.CreatedAtUtc,
                lines,
                lines.Sum(l => l.Price),
                lines.Count == 0 ? CatGirl.DefaultDurationHours : lines.Max(l => l.DurationHours),
                order.CustomerName,
                order.CustomerEmail,
                order.CustomerPhone);
        }
    }
}
