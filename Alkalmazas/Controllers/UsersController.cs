using Alkalmazas.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using MySql.Data.MySqlClient;

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
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("A felhasználónév és a jelszó kötelező.");
            }

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
            catch (DbUpdateException ex) when (ex.InnerException is MySqlException { Number: 1062 })
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
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("A felhasználónév kötelező.");
            }

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
            catch (DbUpdateException ex) when (ex.InnerException is MySqlException { Number: 1062 })
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

        // SAJÁT FÜGGVÉNY: POST api/users/login   (bejelentkezés JSON törzzsel)
        [HttpPost("login")]
        public async Task<ActionResult<UserResponse>> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("A felhasználónév és a jelszó kötelező.");
            }

            var name = request.Name.Trim();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Name == name);

            if (user == null)
            {
                return Unauthorized("Hibás felhasználónév vagy jelszó.");
            }

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Hibás felhasználónév vagy jelszó.");
            }

            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash = _hasher.HashPassword(user, request.Password);
                await _db.SaveChangesAsync();
            }

            return Ok(new UserResponse(user.Id, user.Name));
        }
    }
}
