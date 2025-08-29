using Grpc.Core;
using Microsoft.Extensions.Logging;
using ToursService.Proto;
using ToursService.UseCases;
using ToursService.Dtos;
using System.Threading.Tasks;

using ProtoPositionDto = ToursService.Proto.PositionDto;
using DomainPositionDto = ToursService.Dtos.PositionDto;

namespace ToursService.Controllers
{
    public class PositionsProtoController : PositionsService.PositionsServiceBase
    {
        private readonly IPositionService _positionService;
        private readonly ILogger<PositionsProtoController> _logger;

        public PositionsProtoController(IPositionService positionService, ILogger<PositionsProtoController> logger)
        {
            _positionService = positionService;
            _logger = logger;
        }

        public override Task<ProtoPositionDto> GetByTouristId(GetByTouristIdRequest request, ServerCallContext context)
        {
            var result = _positionService.GetByTouristId(request.TouristId);
            if (result.IsFailed)
            {
                throw new RpcException(new Status(StatusCode.NotFound, string.Join(", ", result.Errors)));
            }

            var pos = result.Value;
            var response = new ProtoPositionDto
            {
                TouristId = pos.TouristId,
                Latitude = pos.Latitude,
                Longitude = pos.Longitude
            };

            return Task.FromResult(response);
        }

        public override Task<ProtoPositionDto> Update(ProtoPositionDto request, ServerCallContext context)
        {
            var dto = new DomainPositionDto
            {
                TouristId = request.TouristId,
                Latitude = request.Latitude,
                Longitude = request.Longitude
            };

            var result = _positionService.Update(dto);
            if (result.IsFailed)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, string.Join(", ", result.Errors)));
            }

            var updated = result.Value;
            var response = new ProtoPositionDto
            {
                TouristId = updated.TouristId,
                Latitude = updated.Latitude,
                Longitude = updated.Longitude
            };

            return Task.FromResult(response);
        }
    }
}
