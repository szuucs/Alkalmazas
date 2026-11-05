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

        [HttpGet("{name}")]
        public async Task<ActionResult<UserResponse>> Login(string name, [FromQuery] string password)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Name == name);

            if (user == null ||
                _hasher.VerifyHashedPassword(user, user.PasswordHash, password) == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Hibás felhasználónév vagy jelszó.");
            }

            return new UserResponse(user.Id, user.Name);
        }

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
                return Conflict("Ez a felhasználónév foglalt.");
            }

            return StatusCode(StatusCodes.Status201Created, new UserResponse(user.Id, user.Name));
        }
    }
}
