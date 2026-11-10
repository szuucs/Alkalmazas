using Alkalmazas.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Text.Json;

namespace Alkalmazas.Controllers
{
    [ApiController]
    [Route("api/catgirls")]
    public class CatGirlsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly string _imageFolder;

        public CatGirlsController(AppDbContext db, IWebHostEnvironment environment)
        {
            _db = db;
            _imageFolder = Path.Combine(environment.ContentRootPath, "App_Data", "Images");
        }

        // GET api/catgirls
        [HttpGet]
        public async Task<ActionResult<List<CatGirl>>> GetAll()
        {
            return await _db.CatGirls.OrderBy(c => c.Name).ToListAsync();
        }

        // GET api/catgirls/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CatGirl>> GetById(int id)
        {
            var cat = await _db.CatGirls.FindAsync(id);

            if (cat == null)
            {
                return NotFound("Nem található a cicálány.");
            }

            return cat;
        }

        // POST api/catgirls
        [HttpPost]
        public Task<ActionResult<CatGirl>> Create(CatGirlRequest request) => CreateCatGirl(request, null);

        // Az űrlap adatait és a választható képet egy kérésben fogadja.
        [HttpPost("with-image")]
        public async Task<ActionResult<CatGirl>> CreateWithImage([FromForm] string details, IFormFile? image)
        {
            // JSON-ban a tizedespont a számítógép nyelvétől függetlenül működik.
            CatGirlRequest? request;
            try { request = JsonSerializer.Deserialize<CatGirlRequest>(details, new JsonSerializerOptions(JsonSerializerDefaults.Web)); }
            catch (JsonException) { return BadRequest("A hirdetés adatai nem olvashatók."); }
            if (request == null) return BadRequest("A hirdetés adatai kötelezők.");
            return await CreateCatGirl(request, image);
        }

        private async Task<ActionResult<CatGirl>> CreateCatGirl(CatGirlRequest request, IFormFile? image)
        {
            var error = InputValidation.CheckCatGirl(request);

            if (error != null)
            {
                return BadRequest(error);
            }

            var cat = new CatGirl();
            Apply(cat, request);

            string? savedFile = null;
            if (image != null)
            {
                var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
                if (extension is not (".jpg" or ".jpeg" or ".png"))
                    return BadRequest("A kép JPG vagy PNG fájl legyen.");

                var fileName = Guid.NewGuid().ToString("N") + extension;
                savedFile = Path.Combine(_imageFolder, fileName);
                cat.ImagePath = "/images/" + fileName;
            }

            try
            {
                if (savedFile != null)
                {
                    await using var output = System.IO.File.Create(savedFile);
                    await image!.CopyToAsync(output);
                }
                _db.CatGirls.Add(cat);
                await _db.SaveChangesAsync();
            }
            catch
            {
                if (savedFile != null && System.IO.File.Exists(savedFile)) System.IO.File.Delete(savedFile);
                throw;
            }

            return CreatedAtAction(nameof(GetById), new { id = cat.Id }, cat);
        }

        // PUT api/catgirls/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult<CatGirl>> Update(int id, CatGirlRequest request)
        {
            var cat = await _db.CatGirls.FindAsync(id);

            if (cat == null)
            {
                return NotFound("Nem található a cicálány.");
            }

            var error = InputValidation.CheckCatGirl(request);

            if (error != null)
            {
                return BadRequest(error);
            }

            Apply(cat, request);
            await _db.SaveChangesAsync();

            return cat;
        }

        // DELETE api/catgirls/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var cat = await _db.CatGirls.FindAsync(id);

            if (cat == null)
            {
                return NotFound("Nem található a cicálány.");
            }

            _db.CatGirls.Remove(cat);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // SAJÁT FÜGGVÉNY 1: GET api/catgirls/search?name=mi&minAge=20&maxAge=30&minHeight=150&maxHeight=175
        // Minden paraméter elhagyható, a megadottak szűkítik a találatokat.
        [HttpGet("search")]
        public async Task<ActionResult<List<CatGirl>>> Search(
            [FromQuery] string? name,
            [FromQuery] int? minAge,
            [FromQuery] int? maxAge,
            [FromQuery] double? minHeight,
            [FromQuery] double? maxHeight)
        {
            var query = _db.CatGirls.AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                var term = name.Trim();
                query = query.Where(c => c.Name.Contains(term));
            }

            if (minAge.HasValue)
            {
                query = query.Where(c => c.Age >= minAge.Value);
            }

            if (maxAge.HasValue)
            {
                query = query.Where(c => c.Age <= maxAge.Value);
            }

            if (minHeight.HasValue)
            {
                query = query.Where(c => c.HeightCm >= minHeight.Value);
            }

            if (maxHeight.HasValue)
            {
                query = query.Where(c => c.HeightCm <= maxHeight.Value);
            }

            return await query.OrderBy(c => c.Name).ToListAsync();
        }

        // SAJÁT FÜGGVÉNY 2: GET api/catgirls/stats   (darabszám és átlagok)
        [HttpGet("stats")]
        public async Task<ActionResult<CatGirlStats>> Stats()
        {
            var count = await _db.CatGirls.CountAsync();

            if (count == 0)
            {
                return new CatGirlStats(0, 0, 0, 0);
            }

            var averageAge = await _db.CatGirls.AverageAsync(c => c.Age);
            var averageHeight = await _db.CatGirls.AverageAsync(c => c.HeightCm);
            var averageWeight = await _db.CatGirls.AverageAsync(c => c.WeightKg);

            return new CatGirlStats(
                count,
                Math.Round(averageAge, 1),
                Math.Round(averageHeight, 1),
                Math.Round(averageWeight, 1));
        }

        private static void Apply(CatGirl cat, CatGirlRequest request)
        {
            cat.Name = request.Name.Trim();
            cat.Age = request.Age;
            cat.HeightCm = request.HeightCm;
            cat.WeightKg = request.WeightKg;
            cat.Phone = request.Phone.Trim();
            cat.Email = request.Email.Trim();
            cat.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        }
    }
}
