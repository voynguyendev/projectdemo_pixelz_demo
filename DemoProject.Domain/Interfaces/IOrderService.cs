using DemoProject.Domain.DTOs;


namespace DemoProject.Domain.Interfaces
{
    public interface IOrderService
    {
         Task<List<OrderDto>> GetOrdersByNameAsync(string? name, CancellationToken ct);
         Task<BaseResponseDTO> CheckoutAsync(string orderId, CancellationToken ct);
    }
}
