using FluentResults;
using PaymentsService.Dtos;

namespace PaymentsService.UseCases
{
    public interface IOrderItemService
    {
        Result<OrderItemDto> Create(OrderItemDto orderItemDto);
        Result Delete(int itemId);
        Result<List<OrderItemDto>> GetAll(long cartId);
        Result<decimal> CalculateTotalPrice(long itemId);
        Result<OrderItemDto> Get(int itemId);
    }
}
