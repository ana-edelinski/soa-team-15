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
        private readonly IImageService _imageService;

        public AuthenticationController(
            IAuthenticationService authenticationService,
            IImageService imageService,
            IWebHostEnvironment webHostEnvironment)
        {
            _authenticationService = authenticationService;
            _imageService = imageService;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpPost("register")]
        public ActionResult Register([FromBody] AccountRegistrationDto account)
        {
            if (!string.IsNullOrEmpty(account.ImageBase64))
            {
                // Konvertuj Base64 u bajtove (uklanja prefiks ako postoji)
                var imageData = Convert.FromBase64String(account.ImageBase64.Split(',')[1]);

                // Definiši folder za slike
                var folderPath = Path.Combine(_webHostEnvironment.WebRootPath ?? "wwwroot", "images", "person");

                // Sačuvaj sliku
                account.ProfilePicture = _imageService.SaveImage(folderPath, imageData, "person");
            }

            var result = _authenticationService.RegisterTourist(account);

            if (result.IsFailed)
            {
                return BadRequest(result.Errors);
            }

            return Ok();
        }
    }
}
