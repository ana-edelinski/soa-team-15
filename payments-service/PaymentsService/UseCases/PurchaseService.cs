using AutoMapper;
using FluentResults;
using PaymentsService.Domain;
using PaymentsService.Domain.RepositoryInterfaces;
using PaymentsService.Dtos;
using System.Security.Cryptography;

namespace PaymentsService.UseCases
{
    public interface IPurchaseService
    {
        Result<List<PurchaseTokenDto>> Checkout(long cartId, long userId);
        Result<bool> HasPurchase(long userId, long tourId);
        Result<List<long>> PurchasedTourIds(long userId);
    }

    public class PurchaseService : IPurchaseService
    {
        private readonly IShoppingCartRepository _cartRepo;
        private readonly IOrderItemRepository _itemRepo;
        private readonly ITourPurchaseTokenRepository _tokenRepo;
        private readonly IMapper _mapper;

        public PurchaseService(
            IShoppingCartRepository cartRepo,
            IOrderItemRepository itemRepo,
            ITourPurchaseTokenRepository tokenRepo,
            IMapper mapper)
        {
            _cartRepo = cartRepo;
            _itemRepo = itemRepo;
            _tokenRepo = tokenRepo;
            _mapper = mapper;
        }

        private static string NewToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(24);
            return Convert.ToBase64String(bytes);
        }

        public Result<List<PurchaseTokenDto>> Checkout(long cartId, long userId)
        {
            var cart = _cartRepo.GetById(cartId);
            if (cart is null) return Result.Fail<List<PurchaseTokenDto>>("Cart not found.");
            if (cart.UserId != userId) return Result.Fail<List<PurchaseTokenDto>>("Forbidden.");

            var toInsert = new List<TourPurchaseToken>();
            foreach (var it in cart.Items)
            {
                if (!_tokenRepo.Exists(userId, it.TourId))
                {
                    toInsert.Add(new TourPurchaseToken
                    {
                        UserId = userId,
                        TourId = it.TourId,
                        Token = NewToken()
                    });
                }
            }

            if (toInsert.Count > 0)
            {
                _tokenRepo.AddRange(toInsert);
                _tokenRepo.SaveChanges();
            }

            // isprazni korpu
            _itemRepo.RemoveByCart(cart.Id);
            _cartRepo.ClearItems(cartId);
            cart.Items.Clear();
            cart.TotalPrice = 0;
            _cartRepo.Update(cart);
            _cartRepo.SaveChanges();

            var result = toInsert
                .Select(t => new PurchaseTokenDto { TourId = t.TourId, Token = t.Token })
                .ToList();

            return Result.Ok(result);
        }

        public Result<bool> HasPurchase(long userId, long tourId)
        {
            return Result.Ok(_tokenRepo.Exists(userId, tourId));
        }

        public Result<List<long>> PurchasedTourIds(long userId)
        {
            return Result.Ok(_tokenRepo.GetPurchasedTourIds(userId));
        }
    }
}
