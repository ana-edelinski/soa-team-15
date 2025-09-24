using AutoMapper;
using FluentResults;
using PaymentsService.Domain;
using PaymentsService.Domain.RepositoryInterfaces;
using PaymentsService.Dtos;
using PaymentsService.Integrations.Saga;
using System.Security.Cryptography;

namespace PaymentsService.UseCases
{
    public interface IPurchaseService
    {
        Result<List<PurchaseTokenDto>> Checkout(long cartId, long userId);
        Result<bool> HasPurchase(long userId, long tourId);
        Result<List<long>> PurchasedTourIds(long userId);

        //SAGA
        Task<ValidatePurchaseReply> ValidatePurchase(long userId, long tourId, long executionId, string correlationId, CancellationToken ct);
        Task<PaymentLockReply> LockPurchase(long userId, long tourId, long executionId, string correlationId, CancellationToken ct);
        Task<PaymentFinalizeReply> FinalizePurchase(long executionId, string correlationId, CancellationToken ct);
        Task<PaymentCompensateReply> CompensatePurchase(long executionId, string reason, string correlationId, CancellationToken ct);
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
                        Status = "Available",
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


        //SAGA


        public async Task<ValidatePurchaseReply> ValidatePurchase(long userId, long tourId, long executionId, string correlationId, CancellationToken ct)
        {
            var exists = _tokenRepo.Exists(userId, tourId);

            if (exists)
                return new ValidatePurchaseReply(ValidatePurchaseStatus.Ok, null, null, correlationId);
            else
                return new ValidatePurchaseReply(ValidatePurchaseStatus.NotFound, "No purchase found", null, correlationId);
        }

        public async Task<PaymentLockReply> LockPurchase(long userId, long tourId, long executionId, string correlationId, CancellationToken ct)
        {
            // 👉 logika: pronađi token (Available) → setuj Status = Locked, ExecutionId = executionId
            var token = await _tokenRepo.GetAvailableAsync(userId, tourId, ct);
            if (token == null)
            {
                return new PaymentLockReply(false, "No available token found", correlationId);
            }

            token.Status = "Locked";
            token.ExecutionId = executionId;
            token.LockedBy = Guid.NewGuid(); // opcionalno možeš koristiti correlationId
            token.LockedAt = DateTime.UtcNow;
            token.ExpiresAt = DateTime.UtcNow.AddMinutes(2); // TTL za lock

            await _tokenRepo.SaveChangesAsync(ct);
            return new PaymentLockReply(true, null, correlationId);
        }

        public async Task<PaymentFinalizeReply> FinalizePurchase(long executionId, string correlationId, CancellationToken ct)
        {
            // 👉 logika: pronađi token (Locked) → setuj Status = Available
            var token = await _tokenRepo.GetByExecutionIdAsync(executionId, ct);
            if (token == null)
            {
                return new PaymentFinalizeReply(false, "Token not found", correlationId);
            }

            if (token.Status != "Locked")
            {
                return new PaymentFinalizeReply(false, $"Token not locked (status={token.Status})", correlationId);
            }

            token.Status = "Available";
            token.LockedBy = null;
            token.LockedAt = null;
            token.ExpiresAt = null;

            await _tokenRepo.SaveChangesAsync(ct);
            return new PaymentFinalizeReply(true, null, correlationId);
        }

        public async Task<PaymentCompensateReply> CompensatePurchase(long executionId, string reason, string correlationId, CancellationToken ct)
        {
            // 👉 logika: pronađi token (Locked) → setuj Status = Available
            var token = await _tokenRepo.GetByExecutionIdAsync(executionId, ct);
            if (token == null)
            {
                return new PaymentCompensateReply(false, "Token not found", correlationId);
            }

            if (token.Status != "Locked")
            {
                return new PaymentCompensateReply(false, $"Token not locked (status={token.Status})", correlationId);
            }

            token.Status = "Available";
            token.ExecutionId = null;
            token.LockedBy = null;
            token.LockedAt = null;
            token.ExpiresAt = null;

            await _tokenRepo.SaveChangesAsync(ct);
            return new PaymentCompensateReply(true, null, correlationId);
        }
    }
}
