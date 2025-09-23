using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentsService.UseCases;

namespace PaymentsService.Controllers
{
    [ApiController]
    [Authorize(Policy = "touristPolicy")]
    [Route("api/shopping")]
    public class CheckoutController : ControllerBase
    {
        private readonly IPurchaseService _purchase;

        public CheckoutController(IPurchaseService purchase) => _purchase = purchase;

        [HttpPost("checkout/{cartId:long}")]
        public ActionResult Checkout(long cartId, [FromQuery] long userId)
        {
            var res = _purchase.Checkout(cartId, userId);
            if (res.IsSuccess) return Ok(res.Value);
            return BadRequest(res.Errors);
        }

        [HttpGet("purchases/has/{userId:long}/{tourId:long}")]
        public ActionResult HasPurchase(long userId, long tourId)
        {
            var res = _purchase.HasPurchase(userId, tourId);
            if (res.IsSuccess) return Ok(res.Value);
            return BadRequest(res.Errors);
        }

        [HttpGet("purchases/{userId:long}")]
        public ActionResult PurchasedIds(long userId)
        {
            var res = _purchase.PurchasedTourIds(userId);
            if (res.IsSuccess) return Ok(res.Value);
            return BadRequest(res.Errors);
        }
    }
}
