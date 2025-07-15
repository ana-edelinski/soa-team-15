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
        public ActionResult<AuthenticationTokensDto> Register([FromBody] AccountRegistrationDto account)
        {
            var result = _authenticationService.RegisterTourist(account);
            if (result.IsFailed)
            {
                return BadRequest(result.Errors);
            }

            return Ok(result.Value); 
        }


        [HttpPost("login")]
        public ActionResult<AuthenticationTokensDto> Login([FromBody] CredentialsDto credentials)
        {
            var result = _authenticationService.Login(credentials);

            if (result.IsFailed)
            {
                return BadRequest("Invalid username or password.");
            }
            return Ok(result.Value);
        }
    }
}
