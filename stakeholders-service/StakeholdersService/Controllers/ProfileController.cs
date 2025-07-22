using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StakeholdersService.Dtos;
using StakeholdersService.UseCases;

namespace StakeholdersService.Controllers
{
    [ApiController]
    [Authorize(Policy ="userPolicy")]
    [Route("api/profile")]
    public class ProfileController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public ProfileController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet("{id}")]
        public ActionResult<UserProfileDto> GetProfile(long id)
        {
            var result = _accountService.GetPersonByUserId(id);
            if (result.IsSuccess) return Ok(result.Value);
            return NotFound(result.Errors);
        }

        [HttpPut]
        public ActionResult<UserProfileDto> UpdateProfile([FromBody] UserProfileDto profileDto)
        {
            var result = _accountService.UpdatePerson(profileDto);
            if (result.IsSuccess) return Ok(result.Value);
            return BadRequest(result.Errors);
        }


    }
}
