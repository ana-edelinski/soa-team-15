using Microsoft.AspNetCore.Mvc;
using StakeholdersService.Dtos;
using StakeholdersService.UseCases;

namespace StakeholdersService.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AuthenticationController(
            IAuthenticationService authenticationService,
            IWebHostEnvironment webHostEnvironment)
        {
            _authenticationService = authenticationService;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpPost("register")]
        public ActionResult Register([FromBody] AccountRegistrationDto account)
        {
            var result = _authenticationService.RegisterTourist(account);

            if (result.IsFailed)
            {
                return BadRequest(result.Errors);
            }

            return Ok();
        }
    }
}
