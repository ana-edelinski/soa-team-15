using AutoMapper;
using FluentResults;
using PaymentsService.Domain;
using PaymentsService.Domain.RepositoryInterfaces;
using PaymentsService.Dtos;

namespace PaymentsService.UseCases
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IShoppingCartRepository _shoppingCartRepository;
        private readonly IMapper _mapper;

        public ShoppingCartService(IShoppingCartRepository shoppingCartRepository, IMapper mapper)
        {
            _shoppingCartRepository = shoppingCartRepository;
            _mapper = mapper;
        }
        public Result<ShoppingCartDto> Create(ShoppingCartDto dto)
        {
            if (dto is null)
                return Result.Fail<ShoppingCartDto>("ShoppingCart DTO is null.");
            if (dto.UserId <= 0)
                return Result.Fail<ShoppingCartDto>("UserId must be a positive number.");

            try
            {
                var cart = _mapper.Map<ShoppingCart>(dto);

                var created = _shoppingCartRepository.Create(cart);

                var createdDto = _mapper.Map<ShoppingCartDto>(created);

                return Result.Ok(createdDto);
            }
            catch (Exception ex)
            {
                return Result.Fail<ShoppingCartDto>($"EXCEPTION: {ex.Message}");
            }
        }


        public Result<List<ShoppingCartDto>> GetAll(long userId)
        {
            try
            {
                var shoppingCarts = _shoppingCartRepository.GetAll(userId);

                var result = shoppingCarts.Select(cart => new ShoppingCartDto
                {
                    Id = cart.Id,
                    UserId = cart.UserId,
                    Items = cart.Items.Select(item => new OrderItemDto
                    {
                        Id = item.Id,
                        TourName = item.TourName,
                        Price = item.Price,
                        TourId = item.TourId,
                        CartId = item.CartId
                    }).ToList(),

                    //PurchaseTokens = cart.PurchaseTokens.Select(token => new TourPurchaseTokenDto
                    //{
                    //    Id = token.Id,
                    //    UserId = token.UserId,
                    //    TourId = token.TourId,
                    //    CartId = token.CartId
                    //}).ToList(),

                    TotalPrice = cart.Items.Sum(item => item.Price)
                }).ToList();

                return Result.Ok(result);
            }
            catch (Exception ex)
            {
                return Result.Fail<List<ShoppingCartDto>>($"Greška prilikom dohvatanja korpi: {ex.Message}");
            }
        }

    }
}
