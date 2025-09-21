using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using ToursService.Domain;          // TransportType
using ToursService.Dtos;            // TransportTimeDto (+ opcioni TransportTimeRequest)
using ToursService.UseCases;        // ITourTransportTimeService

namespace ToursService.Controllers
{
    // Ako nemaš poseban request DTO, može i ovdje:
    public class TransportTimeRequest
    {
        public TransportType Type { get; set; }  // Walk=0, Bike=1, Car=2
        public int Minutes { get; set; }
    }

    [ApiController]
    [Authorize(Policy = "authorPolicy")]
    [Route("api/author/tour/{tourId:long}/transport-times")]
    public class TourTransportTimeController : ControllerBase
    {
        private readonly ITourTransportTimeService _service;

        public TourTransportTimeController(ITourTransportTimeService service)
        {
            _service = service;
        }

        // GET: api/author/tour/{tourId}/transport-times
        [HttpGet]
        public ActionResult<List<TourTransportTimeDto>> GetAll([FromRoute] long tourId)
        {
            var result = _service.GetAll(tourId);
            if (result.IsSuccess) return Ok(result.Value);
            return BadRequest(new { errors = result.Errors.Select(e => e.Message) });
        }

        // GET: api/author/tour/{tourId}/transport-times/{type}
        [HttpGet("{type:int}")]
        public ActionResult<TourTransportTimeDto> GetOne([FromRoute] long tourId, [FromRoute] int type)
        {
            var result = _service.GetOne(tourId, (TransportType)type);
            if (result.IsSuccess && result.Value != null) return Ok(result.Value);

            var msg = result.Errors.FirstOrDefault()?.Message ?? "Transport time not found.";
            return NotFound(new { error = msg });
        }

        // POST: api/author/tour/{tourId}/transport-times
        [HttpPost]
        public IActionResult Create([FromRoute] long tourId, [FromBody] TransportTimeRequest req)
        {
            var claimIdStr = User.FindFirst("id")?.Value;
            if (!long.TryParse(claimIdStr, out var authorId)) return Forbid();

            var result = _service.Create(tourId, authorId, req.Type, req.Minutes);
            if (result.IsSuccess)
                return CreatedAtAction(nameof(GetOne), new { tourId, type = (int)req.Type }, null);

            var msg = result.Errors.FirstOrDefault()?.Message ?? "Error";
            if (msg.Contains("already exists", System.StringComparison.OrdinalIgnoreCase))
                return Conflict(new { error = msg });

            return BadRequest(new { error = msg });
        }

        // PUT: api/author/tour/{tourId}/transport-times
        [HttpPut]
        public IActionResult Update([FromRoute] long tourId, [FromBody] TransportTimeRequest req)
        {
            var claimIdStr = User.FindFirst("id")?.Value;
            if (!long.TryParse(claimIdStr, out var authorId)) return Forbid();

            var result = _service.Update(tourId, authorId, req.Type, req.Minutes);
            if (result.IsSuccess) return NoContent();

            var msg = result.Errors.FirstOrDefault()?.Message ?? "Error";
            if (msg.Contains("not found", System.StringComparison.OrdinalIgnoreCase))
                return NotFound(new { error = msg });

            return BadRequest(new { error = msg });
        }

        // DELETE: api/author/tour/{tourId}/transport-times/{type}
        [HttpDelete("{type:int}")]
        public IActionResult Delete([FromRoute] long tourId, [FromRoute] int type)
        {
            var claimIdStr = User.FindFirst("id")?.Value;
            if (!long.TryParse(claimIdStr, out var authorId)) return Forbid();

            var result = _service.Delete(tourId, authorId, (TransportType)type);
            if (result.IsSuccess) return NoContent();

            var msg = result.Errors.FirstOrDefault()?.Message ?? "Error";
            if (msg.Contains("not found", System.StringComparison.OrdinalIgnoreCase))
                return NotFound(new { error = msg });

            return BadRequest(new { error = msg });
        }
    }
}
