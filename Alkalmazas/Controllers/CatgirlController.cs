using Microsoft.AspNetCore.Mvc;


namespace Alkalmazas.Controllers
{
    [ApiController]
    [Route("api/catgirls")]
    public class CatgirlController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok();
        }
    }
}
