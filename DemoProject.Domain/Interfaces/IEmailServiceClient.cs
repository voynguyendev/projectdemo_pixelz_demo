using DemoProject.Domain.DTOs;
using DemoProject.Domain.Entities;


namespace DemoProject.Domain.Interfaces
{
    public interface IEmailServiceClient
    {
        Task<BaseResponseDTO> SendOrderConfirmationAsync(Order order, CancellationToken ct);
    }
}
