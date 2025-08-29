using Grpc.Core;
using StakeholdersService.UseCases;

namespace StakeholdersService.Controllers
{
    public class AuthenticationProtoController : StakeholdersService.StakeholdersServiceBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly ILogger<AuthenticationProtoController> _logger;

        public AuthenticationProtoController(
            IAuthenticationService authenticationService,
            ILogger<AuthenticationProtoController> logger)
        {
            _authenticationService = authenticationService;
            _logger = logger;
        }

        public override Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
        {
            var result = _authenticationService.Login(new Dtos.CredentialsDto
            {
                Username = request.Username,
                Password = request.Password
            });

            if (result.IsFailed)
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid username or password"));
            }

            var tokens = result.Value;
            return Task.FromResult(new LoginResponse
            {
                Id = tokens.Id,
                AccessToken = tokens.AccessToken
            });
        }

        public override Task<LoginResponse> RegisterTourist(RegisterRequest request, ServerCallContext context)
        {
            var result = _authenticationService.RegisterTourist(new Dtos.AccountRegistrationDto
            {
                Username = request.Username,
                Password = request.Password,
                Email = request.Email,
                Role = request.Role
            });

            if (result.IsFailed)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Registration failed"));
            }

            var tokens = result.Value;
            return Task.FromResult(new LoginResponse
            {
                Id = tokens.Id,
                AccessToken = tokens.AccessToken
            });
        }

    }
}
