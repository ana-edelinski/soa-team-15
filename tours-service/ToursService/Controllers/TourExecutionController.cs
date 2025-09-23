using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToursService.Dtos;
using ToursService.UseCases;

namespace ToursService.Controllers
{
    [ApiController]
    [Authorize(Policy = "touristPolicy")]
    [Route("api/execution")]
    public class TourExecutionController : ControllerBase
    {
        private readonly ITourExecutionService _executionService;
        private readonly TourStartSagaOrchestrator _orchestrator;


        public TourExecutionController(ITourExecutionService tourService, TourStartSagaOrchestrator orchestrator)
        {
            _executionService = tourService;
            _orchestrator = orchestrator;
        }

        [HttpPost]
        public IActionResult Create([FromBody] TourExecutionDto tour)
        {
            if (tour is null) return BadRequest("Body is required.");

            var result = _executionService.Create(tour);
            if (result.IsSuccess) return Ok(result.Value);

            return BadRequest(result.Errors);
        }

        //[HttpPost("complete/{executionId:long}")]
        //public IActionResult CompleteTourExecution(long executionId)
        //{
        //    var result = _executionService.CompleteTourExecution(executionId);
        //    if (result is null) return NotFound($"TourExecution with ID {executionId} not found.");

        //    if (result.IsSuccess) return Ok(result.Value);
        //    return BadRequest(result.Errors);
        //}

        //[HttpPost("abandon/{executionId:long}")]
        //public IActionResult AbandonTourExecution(long executionId)
        //{
        //    var result = _executionService.AbandonTourExecution(executionId);
        //    if (result is null) return NotFound($"TourExecution with ID {executionId} not found.");

        //    if (result.IsSuccess) return Ok(result.Value);
        //    return BadRequest(result.Errors);
        //}

        [HttpPost("complete/{executionId:long}")]
        public async Task<IActionResult> CompleteTourExecution(long executionId, CancellationToken ct)
        {
            var result = _executionService.CompleteTourExecution(executionId);
            if (result is null) return NotFound($"TourExecution with ID {executionId} not found.");
            if (!result.IsSuccess) return BadRequest(result.Errors);

            await _orchestrator.NotifyFinalizeAsync(executionId, ct);
            return Ok(result.Value);
        }

        [HttpPost("abandon/{executionId:long}")]
        public async Task<IActionResult> AbandonTourExecution(long executionId, CancellationToken ct)
        {
            var result = _executionService.AbandonTourExecution(executionId);
            if (result is null) return NotFound($"TourExecution with ID {executionId} not found.");
            if (!result.IsSuccess) return BadRequest(result.Errors);

            await _orchestrator.NotifyFinalizeAsync(executionId, ct);
            return Ok(result.Value);
        }


        [HttpGet("by_tour_and_tourist/{touristId:long}/{tourId:long}")]
        public ActionResult<TourExecutionDto> GetByTourAndTouristId(long touristId, long tourId)
        {
            var result = _executionService.GetByTourAndTouristId(touristId, tourId);
            if (result == null)
                return NotFound("Tour execution not found for the specified tourist and tour.");

            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.Errors);
        }

        [HttpGet("active/{touristId:long}")]
        public ActionResult<TourExecutionDto> GetActiveTourByTouristId(long touristId)
        {
            var result = _executionService.GetActiveTourByTouristId(touristId);

            if (result.IsSuccess) return Ok(result.Value);

            if (result.IsFailed)
            {
                var msg = result.Errors.FirstOrDefault()?.Message ?? "Not found";
                if (msg.Contains("no active tour found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { message = msg });

                return BadRequest(result.Errors);
            }

            return BadRequest();
        }

        [HttpPut("completeKeyPoint/{executionId:long}/{keyPointId:long}")]
        public ActionResult<TourExecutionDto> CompleteKeyPoint(long executionId, long keyPointId)
        {
            try
            {
                var result = _executionService.CompleteKeyPoint(executionId, keyPointId);

                return result.IsFailed
                    ? Conflict(new { message = result.Errors.First().Message })
                    : Ok(result.Value);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpPut("updateLastActivity/{executionId:long}")]
        public IActionResult UpdateLastActivity(long executionId)
        {
            try
            {
                _executionService.UpdateLastActivity(executionId);

                return Ok(new { message = "Last activity updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the last activity.", details = ex.Message });
            }
        }

        [HttpGet("{tourId}/keypoints")]
        public IActionResult GetKeyPoints(long tourId)
        {
            var keyPoints = _executionService.GetKeyPointsForTour(tourId);
            return Ok(keyPoints);
        }

        [HttpPost("saga/create")]
        public async Task<IActionResult> Create([FromBody] TourExecutionDto tour, CancellationToken ct)
        {
            if (tour is null) return BadRequest("Body is required.");

            // Ovde ne ide _executionService.Create(tour), već orkestrator
            var result = await _orchestrator.StartTourSagaAsync(
                userId: tour.TouristId,   // pretpostavljam da imaš ovo polje u DTO
                tourId: tour.TourId,
                locationId: tour.LocationId, // ako ga imaš u DTO
                ct: ct
            );

            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

    }
}