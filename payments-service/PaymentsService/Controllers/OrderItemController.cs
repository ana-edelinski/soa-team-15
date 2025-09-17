using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentsService.Dtos;
using PaymentsService.UseCases;

namespace PaymentsService.Controllers
{
    [ApiController]
    [Authorize(Policy = "touristPolicy")]
    [Route("api/shopping/item")]
    public class OrderItemController : ControllerBase
    {
        private readonly IOrderItemService _orderItemService;

        public OrderItemController(IOrderItemService orderItemService)
        {
            _orderItemService = orderItemService;
        }

        [HttpPost]
        public ActionResult<OrderItemDto> Create([FromBody] OrderItemDto item)
        {
            var result = _orderItemService.Create(item);
            if (result.IsSuccess) return Ok(result.Value);
            return BadRequest(result.Errors);
        }

        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            var result = _orderItemService.Delete(id);
            if (result.IsSuccess) return Ok();
            return BadRequest(result.Errors);
        }

        [HttpGet("getbyid/{id:int}")]
        public ActionResult<OrderItemDto> Get(int id)
        {
            var result = _orderItemService.Get(id);
            if (result.IsSuccess) return Ok(result.Value);
            return NotFound(result.Errors);
        }

        [HttpGet("getPrice/{id:long}")]
        public ActionResult<decimal> CalculateTotalPrice(long id)
        {
            var result = _orderItemService.CalculateTotalPrice(id);
            if (result.IsSuccess) return Ok(result.Value);
            return BadRequest(result.Errors);
        }

        [HttpGet("getAllFromCart/{id:long}")]
        public ActionResult<List<OrderItemDto>> GetAll(long id)
        {
            var result = _orderItemService.GetAll(id);
            if (result.IsSuccess) return Ok(result.Value);
            return BadRequest(result.Errors);
        }
    }
}