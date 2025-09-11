using AutoMapper;
using FluentResults;
using ToursService.Domain;
using ToursService.Domain.RepositoryInterfaces;
using ToursService.Dtos;

namespace ToursService.UseCases
{
    public class PositionService: IPositionService
    {
        private readonly IPositionRepository _positionRepository;
        private readonly IMapper _mapper;

        public PositionService(IPositionRepository positionRepository, IMapper mapper)
        {
            _positionRepository = positionRepository;
            _mapper = mapper;
        }

        public Result<PositionDto> GetByTouristId(long touristId)
        {
            if (touristId <= 0)
                return Result.Fail<PositionDto>("Invalid tourist id.");

            try
            {
                var pos = _positionRepository.GetByTouristId(touristId);
                if (pos == null)
                    return Result.Fail<PositionDto>("No position found for this tourist.");

                return Result.Ok(_mapper.Map<PositionDto>(pos));
            }
            catch (Exception ex)
            {
                return Result.Fail<PositionDto>($"EXCEPTION: {ex.Message}");
            }
        }

        public Result<PositionDto> Update(PositionDto dto)
        {
            if (dto == null)
                return Result.Fail<PositionDto>("Position DTO is null.");

            if (dto.TouristId <= 0)
                return Result.Fail<PositionDto>("Invalid tourist id.");

            try
            {
                var entity = new Position(dto.TouristId, dto.Latitude, dto.Longitude);

                _positionRepository.Update(entity);

                return Result.Ok(new PositionDto
                {
                    TouristId = entity.TouristId,
                    Latitude = entity.Latitude,
                    Longitude = entity.Longitude
                });
            }
            catch (Exception ex)
            {
                return Result.Fail<PositionDto>($"EXCEPTION: {ex.Message}");
            }
        }
    }
}
