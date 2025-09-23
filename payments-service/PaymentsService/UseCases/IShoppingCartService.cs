using PaymentsService.Dtos;
using FluentResults;

namespace PaymentsService.UseCases
{
    public interface IShoppingCartService
    {
        Result<ShoppingCartDto> Create(ShoppingCartDto orderItemDto);
        Result<List<ShoppingCartDto>> GetAll(long userId);
    }
}
