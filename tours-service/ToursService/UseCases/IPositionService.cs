using FluentResults;
using ToursService.Dtos;

namespace ToursService.UseCases
{
    public interface IPositionService
    {
        Result<PositionDto> GetByTouristId(long touristId);
        Result<PositionDto> Update(PositionDto dto);
    }
}
