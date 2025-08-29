using Grpc.Core;
using Microsoft.Extensions.Logging;
using ToursService.Proto;
using ToursService.UseCases;
using ToursService.Dtos;
using System.Linq;
using System.Threading.Tasks;
using ProtoTourDto = ToursService.Proto.TourDto;
using DomainTourDto = ToursService.Dtos.TourDto;

namespace ToursService.Controllers
{
    public class ToursProtoController : ToursService.Proto.ToursService.ToursServiceBase
    {
        private readonly ITourService _tourService;
        private readonly ILogger<ToursProtoController> _logger;

        public ToursProtoController(ITourService tourService, ILogger<ToursProtoController> logger)
        {
            _tourService = tourService;
            _logger = logger;
        }

        public override Task<ProtoTourDto> Create(ProtoTourDto request, ServerCallContext context)
        {
            // Mapiramo proto -> domain DTO
            var dto = new DomainTourDto
            {
                Id = request.Id,
                Name = request.Name,
                Description = request.Description,
                Difficulty = request.Difficulty,
                Tags = request.Tags.Select(tag => (Dtos.TourTags)tag).ToList(),
                UserId = request.UserId,
                Status = (Dtos.TourStatus)request.Status,
                Price = request.Price
            };

            var result = _tourService.Create(dto);
            if (result.IsFailed)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, string.Join(", ", result.Errors)));
            }

            var created = result.Value;

            // Mapiramo domain DTO -> proto
            var response = new ProtoTourDto
            {
                Id = created.Id,
                Name = created.Name,
                Description = created.Description,
                Difficulty = created.Difficulty,
                Tags = { created.Tags.Select(t => (Proto.TourTags)t) },
                UserId = created.UserId,
                Status = (Proto.TourStatus)created.Status,
                Price = created.Price
            };

            return Task.FromResult(response);
        }

        public override Task<ToursListResponse> GetByUserId(GetByUserIdRequest request, ServerCallContext context)
        {
            var result = _tourService.GetByUserId(request.UserId);
            if (result.IsFailed)
            {
                throw new RpcException(new Status(StatusCode.NotFound, string.Join(", ", result.Errors)));
            }

            var response = new ToursListResponse();
            foreach (var tour in result.Value)
            {
                response.Tours.Add(new ProtoTourDto
                {
                    Id = tour.Id,
                    Name = tour.Name,
                    Description = tour.Description,
                    Difficulty = tour.Difficulty,
                    Tags = { tour.Tags.Select(t => (Proto.TourTags)t) },
                    UserId = tour.UserId,
                    Status = (Proto.TourStatus)tour.Status,
                    Price = tour.Price
                });
            }

            return Task.FromResult(response);
        }
    }
}
