using DemoProject.Domain.DTOs;
using DemoProject.Domain.Entities;



namespace DemoProject.Domain.Interfaces
{
    public interface IProductionServiceClient
    {
        Task<BaseResponseDTO> PushOrderAsync(Order order, CancellationToken ct);
    }
}
