using AutoMapper;
using FluentResults;
using PaymentsService.Domain;
using PaymentsService.Domain.RepositoryInterfaces;
using PaymentsService.Dtos;

namespace PaymentsService.UseCases
{
    public class OrderItemService : IOrderItemService
    {
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IMapper _mapper;

        public OrderItemService(IOrderItemRepository orderItemRepository, IMapper mapper)
        {
            _orderItemRepository = orderItemRepository;
            _mapper = mapper;
        }

        public Result<OrderItemDto> Create(OrderItemDto dto)
        {
            if (dto is null)
                return Result.Fail<OrderItemDto>("OrderItem DTO is null.");
            if (dto.CartId <= 0)
                return Result.Fail<OrderItemDto>("CartId must be a positive number.");
            if (dto.TourId <= 0)
                return Result.Fail<OrderItemDto>("TourId must be a positive number.");
            if (string.IsNullOrWhiteSpace(dto.TourName))
                return Result.Fail<OrderItemDto>("TourName is required.");

            try
            {
                var existingItems = _orderItemRepository.GetAll(dto.CartId);
                if (existingItems.Any(i => i.TourId == dto.TourId))
                {
                    return Result.Fail<OrderItemDto>(
                        $"Tour with ID={dto.TourId} is already in the cart."
                    );
                }

                var orderItem = _mapper.Map<OrderItem>(dto);

                var created = _orderItemRepository.Create(orderItem);

                var createdDto = _mapper.Map<OrderItemDto>(created);

                return Result.Ok(createdDto);
            }
            catch (Exception ex)
            {
                return Result.Fail<OrderItemDto>($"EXCEPTION: {ex.Message}");
            }
        }


        public Result Delete(int itemId)
        {
            if (itemId <= 0)
                return Result.Fail("Invalid itemId.");

            try
            {
                var success = _orderItemRepository.Delete(itemId);

                if (!success)
                    return Result.Fail($"Order item with Id={itemId} not found.");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail($"EXCEPTION: {ex.Message}");
            }
        }

        public Result<decimal> CalculateTotalPrice(long itemId)
        {
            try
            {
                var result = _orderItemRepository.CalculateTotalPrice(itemId);

                if (result == -1)
                    return Result.Fail<decimal>("Order item not found.");

                return Result.Ok(result);
            }
            catch (Exception ex)
            {
                return Result.Fail<decimal>($"Greška prilikom računanja cene stavke: {ex.Message}");
            }
        }

        public Result<List<OrderItemDto>> GetAll(long cartId)
        {
            try
            {
                var orderItems = _orderItemRepository.GetAll(cartId);

                var result = orderItems.Select(item => new OrderItemDto
                {
                    Id = item.Id,
                    TourName = item.TourName,
                    Price = item.Price,
                    TourId = item.TourId,
                    CartId = item.CartId
                }).ToList();

                return Result.Ok(result);
            }
            catch (Exception ex)
            {
                return Result.Fail<List<OrderItemDto>>($"Greška prilikom dohvatanja stavki: {ex.Message}");
            }
        }

        public Result<OrderItemDto> Get(int itemId)
        {
            if (itemId <= 0)
                return Result.Fail<OrderItemDto>("Invalid itemId.");

            try
            {
                var item = _orderItemRepository.Get(itemId);

                if (item == null)
                    return Result.Fail<OrderItemDto>($"Order item with Id={itemId} not found.");

                var dto = _mapper.Map<OrderItemDto>(item);

                return Result.Ok(dto);
            }
            catch (Exception ex)
            {
                return Result.Fail<OrderItemDto>($"EXCEPTION: {ex.Message}");
            }
        }
    }
}
