using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StakeholdersService.Dtos;
using StakeholdersService.UseCases;

namespace StakeholdersService.Controllers
{
    [ApiController]
    [Authorize(Policy = "administratorPolicy")]
    [Route("api/administration/account")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        public ActionResult<PagedResult<AccountDto>> GetAllAccount([FromQuery] int page, [FromQuery] int pageSize)
        {
            var result = _accountService.GetPagedAccount(page, pageSize);
            if (result.IsSuccess) return Ok(result.Value);
            return BadRequest(result.Errors);
        }

        [HttpPut("block")]
        public ActionResult<AccountDto> BlockUser([FromBody] AccountDto account)
        {
            var result = _accountService.BlockUser(account);
            if (result.IsSuccess) return Ok(result.Value);
            return BadRequest(result.Errors);
        }
    }
}
