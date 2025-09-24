using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using ToursService.Dtos;
using ToursService.UseCases;

namespace ToursService.Controllers
{
    [ApiController]
    [Route("api/public/tours")]
    [AllowAnonymous] // dozvoli prolaz do metode; mi ćemo RUČNO provjeriti token + rolu
    public class PublicToursController : ControllerBase
    {
        private readonly ITourService _tourService;
        public PublicToursController(ITourService tourService) => _tourService = tourService;

        [HttpGet]
        public ActionResult<List<TourPublicDto>> GetAll()
        {
            if (!IsTourist(User)) return Forbid(); // ili Unauthorized() ako želiš drugačije
            var r = _tourService.GetPublishedPublic();
            return r.IsSuccess ? Ok(r.Value) : BadRequest(r.Errors);
        }

        [HttpGet("{id:long}")]
        public ActionResult<TourPublicDto> GetOne(long id)
        {
            if (!IsTourist(User)) return Forbid();
            var r = _tourService.GetPublicTour(id);
            if (r.IsSuccess) return Ok(r.Value);
            return NotFound(new { error = r.Errors.FirstOrDefault()?.Message ?? "Not found" });
        }

        private static bool IsTourist(ClaimsPrincipal user)
        {
            if (!(user?.Identity?.IsAuthenticated ?? false)) return false;

            // pokrij sve tipične claim tipove za role
            var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value)
                .Concat(user.FindAll("role").Select(c => c.Value))
                .Concat(user.FindAll("roles").Select(c => c.Value));

            return roles.Any(r => string.Equals(r, "Tourist", StringComparison.OrdinalIgnoreCase));
        }
    }
}
