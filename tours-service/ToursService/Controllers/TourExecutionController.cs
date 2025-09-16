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

        public TourExecutionController(ITourExecutionService tourService)
        {
            _executionService = tourService;
        }

        [HttpPost]
        public IActionResult Create([FromBody] TourExecutionDto tour)
        {
            if (tour is null) return BadRequest("Body is required.");

            var result = _executionService.Create(tour);
            if (result.IsSuccess) return Ok(result.Value);

            return BadRequest(result.Errors);
        }

        [HttpPost("complete/{executionId:long}")]
        public IActionResult CompleteTourExecution(long executionId)
        {
            var result = _executionService.CompleteTourExecution(executionId);
            if (result is null) return NotFound($"TourExecution with ID {executionId} not found.");

            if (result.IsSuccess) return Ok(result.Value);
            return BadRequest(result.Errors);
        }

        [HttpPost("abandon/{executionId:long}")]
        public IActionResult AbandonTourExecution(long executionId)
        {
            var result = _executionService.AbandonTourExecution(executionId);
            if (result is null) return NotFound($"TourExecution with ID {executionId} not found.");

            if (result.IsSuccess) return Ok(result.Value);
            return BadRequest(result.Errors);
        }
    }
}