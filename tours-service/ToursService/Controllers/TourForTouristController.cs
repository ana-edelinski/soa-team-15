using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToursService.Dtos;
using ToursService.UseCases;

namespace ToursService.Controllers
{
    [ApiController]
    [Authorize(Policy = "touristPolicy")]
    [Route("api/tour")]
    public class TourForTouristController: ControllerBase
    {

        private readonly ITourService _tourService;

        public TourForTouristController(ITourService tourService)
        {
            _tourService = tourService;
        }

        [HttpGet("{id:long}")]
        public ActionResult<TourForTouristDto> GetTourWithKeyPoints(long id)
        {
            var result = _tourService.GetTourWithKeyPoints(id);
            return result.IsSuccess
                ? Ok(result.Value)
                : NotFound(new { message = result.Errors.First().Message });
        }

        [HttpGet("published")] // za sada se dobavljaju publikovane kad se napravi kupovina onda kupljene
        public ActionResult<List<TourDto>> GetPublished()
        {
            var result = _tourService.GetPublished();
            if (result.IsSuccess) return Ok(result.Value);
            return BadRequest(result.Errors.FirstOrDefault()?.Message ?? "Failed to fetch published tours.");
        }
    }
}
