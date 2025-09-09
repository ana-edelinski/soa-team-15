using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ToursService.Dtos;
using ToursService.UseCases;

namespace ToursService.Controllers
{

    [ApiController]
    [Authorize] // prilagodi: npr. [Authorize(Policy = "touristPolicy")]
    [Route("api/tours/{tourId:long}/reviews")]
    public class TourReviewController : ControllerBase
    {
        private readonly ITourReviewService _service;

        public TourReviewController(ITourReviewService service)
        {
            _service = service;
        }

        [HttpPost]
        public ActionResult<TourReviewDto> Create(long tourId, [FromBody] TourReviewCreateDto dto)
        {
            // enforce path vs body
            dto.IdTour = tourId;

            // iz claim-a (ako šalješ "id" u JWT)
            long? requesterId = null;
            var idClaim = User.FindFirst("id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(idClaim, out var parsed)) requesterId = parsed;

            var result = _service.Create(dto, requesterId);
            if (result.IsSuccess) return Ok(result.Value);
            return BadRequest(result.Errors);
        }

        [HttpGet]
        [AllowAnonymous] // čitanje može biti javno; prilagodi po potrebi
        public ActionResult<List<TourReviewDto>> GetByTour(long tourId)
        {
            var result = _service.GetByTour(tourId);
            if (result.IsSuccess) return Ok(result.Value);
            return BadRequest(result.Errors);
        }

        [HttpGet("summary")]
        [AllowAnonymous]
        public ActionResult<TourReviewSummaryDto> Summary(long tourId)
        {
            var result = _service.SummaryByTour(tourId);
            if (result.IsSuccess) return Ok(result.Value);
            return BadRequest(result.Errors);
        }

        // --- dodatni endpointi ---

        [HttpGet("~/api/reviews/by-tourist/{touristId:long}")]
        public ActionResult<List<TourReviewDto>> GetByTourist(long touristId)
        {
            var claimIdStr = User.FindFirst("id")?.Value;
            if (!long.TryParse(claimIdStr, out var claimId)) return Forbid();

            if (!User.IsInRole("Administrator") && claimId != touristId) return Forbid();

            var result = _service.GetByTourist(touristId);
            if (result.IsSuccess) return Ok(result.Value);
            return BadRequest(result.Errors);
        }

        [HttpGet("~/api/reviews")]
        [Authorize(Roles = "Administrator")]
        public ActionResult<PagedResult<TourReviewDto>> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = _service.GetPaged(page, pageSize);
            if (result.IsSuccess) return Ok(result.Value);
            return BadRequest(result.Errors);
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(long tourId, int id)
        {
            var claimIdStr = User.FindFirst("id")?.Value;
            if (!long.TryParse(claimIdStr, out var requesterId)) return Forbid();

            // (opciono) možeš proveriti da review pripada toj turi u servisu/repo sloju
            var result = _service.Delete(id, requesterId);
            if (result.IsSuccess) return NoContent();
            if (result.Errors.Any(e => e.Message == "Forbidden.")) return Forbid();
            return NotFound(result.Errors);
        }
    }
}
