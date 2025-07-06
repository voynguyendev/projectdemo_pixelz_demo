using DemoProject.Domain.DTOs;
using DemoProject.Domain.Entities;


namespace DemoProject.Domain.Interfaces
{
    public interface IPaymentServiceClient
    {
        Task<BaseResponseDTO> ProcessPaymentAsync(Order order, CancellationToken ct);
    }
}
