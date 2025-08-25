using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToursService.Dtos;
using ToursService.UseCases;

namespace ToursService.Controllers
{
    [ApiController]
    [Authorize(Policy = "touristPolicy")]
    [Route("api/position")]
    public class PositionController: ControllerBase
    {
        private readonly IPositionService _positionService;

        public PositionController(IPositionService positionService) 
        {
            _positionService = positionService;
        }

        [HttpGet("{id:long}")]
        public ActionResult<PositionDto> GetByUserId(long id)
        {
            var claimIdStr = User.FindFirst("id")?.Value;
            if (!long.TryParse(claimIdStr, out var claimId)) return Forbid();

            
            if (id != claimId) return Forbid();

            var result = _positionService.GetByTouristId(id);
            if (result.IsSuccess) return Ok(result.Value);

            return NotFound(result.Errors);
        }

        [HttpPut]
        public IActionResult Update([FromBody] PositionDto dto)
        {
            if (dto is null) return BadRequest("Body is required.");

            var claimIdStr = User.FindFirst("id")?.Value;
            if (!long.TryParse(claimIdStr, out var claimId)) return Forbid();

            
            if (dto.TouristId != claimId) return Forbid();

            var result = _positionService.Update(dto);
            if (result.IsSuccess) return Ok(result.Value); 

            return BadRequest(result.Errors);
        }
    }
}
