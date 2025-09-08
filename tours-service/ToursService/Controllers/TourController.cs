using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToursService.Dtos;
using ToursService.UseCases;

namespace ToursService.Controllers
{
    [ApiController]
    [Authorize(Policy = "authorPolicy")]
    [Route("api/author/tour")]
    public class TourController : ControllerBase
    {
        private readonly ITourService _tourService;

        public TourController(ITourService tourService)
        {
            _tourService = tourService;
        }

        [HttpPost]
        public ActionResult<TourDto> Create([FromBody] TourDto tour)
        {
            var result = _tourService.Create(tour);
            if (result.IsSuccess) return Ok(result.Value);
            return BadRequest(result.Errors);
        }

        [HttpGet("{id:int}")]
        public ActionResult<List<TourDto>> GetAllByUserId(int id)
        {
            var claimIdStr = User.FindFirst("id")?.Value;
            if (!long.TryParse(claimIdStr, out var claimId))
                return Forbid();

            if (!User.IsInRole("Administrator") && id != claimId)
                return Forbid();

            var result = _tourService.GetByUserId(id);
            if (result.IsSuccess) return Ok(result.Value);
            return BadRequest(result.Errors);
        }

        [HttpGet("published")]
        [AllowAnonymous] // ili [Authorize(Roles = "Tourist,Administrator")]

        public ActionResult<List<TourDto>> GetPublished()
        {
            var result = _tourService.GetPublished();
            if (result.IsSuccess) return Ok(result.Value);
            return BadRequest(result.Errors.FirstOrDefault()?.Message ?? "Failed to fetch published tours.");
        }
    }
}
