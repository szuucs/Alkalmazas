using Alkalmazas.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Alkalmazas.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly PasswordHasher<User> _hasher = new();

        public UsersController(AppDbContext db)
        {
            _db = db;
        }

        // GET api/users
        [HttpGet]
        public async Task<ActionResult<List<UserResponse>>> GetAll()
        {
            return await _db.Users
                .OrderBy(u => u.Name)
                .Select(u => new UserResponse(u.Id, u.Name))
                .ToListAsync();
        }

        // GET api/users/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserResponse>> GetById(int id)
        {
            var user = await _db.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound("Nem található a felhasználó.");
            }

            return new UserResponse(user.Id, user.Name);
        }

        // POST api/users   (regisztráció)
        [HttpPost]
        public async Task<ActionResult<UserResponse>> Register(RegisterRequest request)
        {
            var name = request.Name.Trim();

            if (name.Length < 3 || name.Length > 64 || request.Password.Length < 6)
            {
                return BadRequest("A név legalább 3, a jelszó legalább 6 karakter legyen.");
            }

            if (await _db.Users.AnyAsync(u => u.Name == name))
            {
                return Conflict("Ez a felhasználónév foglalt.");
            }

            var user = new User { Name = name };
            user.PasswordHash = _hasher.HashPassword(user, request.Password);

            _db.Users.Add(user);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // két egyszerre érkező regisztráció ugyanazzal a névvel
                return Conflict("Ez a felhasználónév foglalt.");
            }

            return CreatedAtAction(nameof(GetById), new { id = user.Id }, new UserResponse(user.Id, user.Name));
        }

        // PUT api/users/5   (név és/vagy jelszó módosítása; üres jelszónál a jelszó nem változik)
        [HttpPut("{id:int}")]
        public async Task<ActionResult<UserResponse>> Update(int id, UpdateUserRequest request)
        {
            var user = await _db.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound("Nem található a felhasználó.");
            }

            var name = request.Name.Trim();

            if (name.Length < 3 || name.Length > 64)
            {
                return BadRequest("A név 3 és 64 karakter között legyen.");
            }

            if (!string.IsNullOrEmpty(request.Password) && request.Password.Length < 6)
            {
                return BadRequest("Az új jelszó legalább 6 karakter legyen.");
            }

            if (await _db.Users.AnyAsync(u => u.Name == name && u.Id != id))
            {
                return Conflict("Ez a felhasználónév foglalt.");
            }

            user.Name = name;

            if (!string.IsNullOrEmpty(request.Password))
            {
                user.PasswordHash = _hasher.HashPassword(user, request.Password);
            }

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict("Ez a felhasználónév foglalt.");
            }

            return new UserResponse(user.Id, user.Name);
        }

        // DELETE api/users/5   (a felhasználó rendelései is törlődnek)
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _db.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound("Nem található a felhasználó.");
            }

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // SAJÁT FÜGGVÉNY: GET api/users/login?name=...&password=...   (bejelentkezés)
        [HttpGet("login")]
        public async Task<ActionResult<UserResponse>> Login([FromQuery] string name, [FromQuery] string password)
        {
            var trimmed = name.Trim();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Name == trimmed);

            if (user == null ||
                _hasher.VerifyHashedPassword(user, user.PasswordHash, password) == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Hibás felhasználónév vagy jelszó.");
            }

            return new UserResponse(user.Id, user.Name);
        }
    }
}
