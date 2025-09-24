using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToursService.Dtos;
using ToursService.UseCases;
using System.Linq;              
using System.Security.Claims;  

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




        [HttpGet("byId/{tourId:long}")]
        public ActionResult<TourDto> GetById([FromRoute] long tourId)
        {
            try
            {
                var result = _tourService.GetById(tourId);
                if (result.IsSuccess && result.Value != null) return Ok(result.Value);

                // 404 sa porukom
                var msg = result.Errors.FirstOrDefault()?.Message ?? "Tour not found.";
                return NotFound(new { error = msg });
            }
            catch (Exception ex)
            {
                // 500 sa porukom (privremeno, dok ne otkloniš uzrok)
                return Problem(detail: ex.Message, statusCode: 500);
            }
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

        [HttpPost("{tourId:long}/publish")]
        public IActionResult Publish(long tourId)
        {
            var claimIdStr = User.FindFirst("id")?.Value;
            if (!long.TryParse(claimIdStr, out var authorId)) return Forbid();

            var result = _tourService.Publish(tourId, authorId);
            if (result.IsFailed)
                return BadRequest(new { errors = result.Errors.Select(e => e.Message).ToList() });

            // nakon updejta učitaj ponovo da dobiješ vrijeme
            var t = _tourService.GetById(tourId);
            if (t.IsSuccess)
                return Ok(new { publishedTime = t.Value.PublishedTime });

            return Ok(new { publishedTime = (DateTime?)null });
        }

        [HttpPost("{tourId:long}/archive")]
        public IActionResult Archive(long tourId)
        {
            var claimIdStr = User.FindFirst("id")?.Value;
            if (!long.TryParse(claimIdStr, out var authorId)) return Forbid();

            var result = _tourService.Archive(tourId, authorId);
            if (result.IsFailed)
                return BadRequest(new { errors = result.Errors.Select(e => e.Message).ToList() });

            var t = _tourService.GetById(tourId);
            if (t.IsSuccess)
                return Ok(new { archiveTime = t.Value.ArchiveTime });

            return Ok(new { archiveTime = (DateTime?)null });
        }

        [HttpPost("{tourId:long}/reactivate")]
        public IActionResult Reactivate(long tourId)
        {
            var claimIdStr = User.FindFirst("id")?.Value;
            if (!long.TryParse(claimIdStr, out var authorId)) return Forbid();

            var result = _tourService.Reactivate(tourId, authorId);
            if (result.IsSuccess) return NoContent();
            return BadRequest(new { errors = result.Errors.Select(e => e.Message).ToList() });
        }


        [HttpGet("getKeyPoints/{tourId:long}")]
        [AllowAnonymous]
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


        [HttpPost("updateTourKm/{tourId:long}")]
        public ActionResult<double> UpdateTourKM([FromRoute] long tourId)
        {
            var result = _tourService.UpdateTourKM(tourId, new List<KeyPointDto>());
            if (result.IsFailed) return BadRequest(result.Errors.Select(e => e.Message));
            return Ok(result.Value);
        }


        [HttpGet("all")]
        [Authorize(Roles = "Administrator")]
        public ActionResult<List<TourDto>> GetAll()
        {
            var result = _tourService.GetAll();
            if (result.IsSuccess) return Ok(result.Value);
            return BadRequest(result.Errors);
        }



        [HttpGet("all-with-unpublished")]
        [AllowAnonymous] // ili [Authorize] ako želiš samo da su prijavljeni
        public ActionResult<List<TourDto>> GetAllIncludingUnpublished()
        {
            var result = _tourService.GetAllIncludingUnpublished();
            if (result.IsSuccess) return Ok(result.Value);
            return BadRequest(result.Errors);
        }
        




        
// helper u istom kontroleru
    private static bool HasTouristRole(ClaimsPrincipal user)
    {
        // pokrij sve tipične claim tipove za role (ClaimTypes.Role, "role", "roles")
        var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value)
            .Concat(user.FindAll("role").Select(c => c.Value))
            .Concat(user.FindAll("roles").Select(c => c.Value));

        return roles.Any(r => string.Equals(r, "Tourist", StringComparison.OrdinalIgnoreCase));
    }


    }


}
