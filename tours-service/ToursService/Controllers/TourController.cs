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
    
        [HttpGet("getKeyPoints/{tourId:long}")]
        public ActionResult<List<KeyPointDto>> GetKeyPointsForTour(long tourId)
        {
           

            var result = _tourService.GetKeyPointsByTour(tourId);
            if (result.IsSuccess) return Ok(result.Value);
            return BadRequest(result.Errors);
        }

        [HttpPost("addKeyPoints/{tourId:long}")]
        [Consumes("multipart/form-data")]
        public IActionResult AddKeyPoint([FromRoute] long tourId, [FromForm] KeyPointDto keyPointDto)
        {
            

            var result = _tourService.AddKeyPoint(tourId, keyPointDto);
            if (result.IsSuccess) return Ok(result.Value);
            return BadRequest(result.Errors);
        }
    }

   
    
}
