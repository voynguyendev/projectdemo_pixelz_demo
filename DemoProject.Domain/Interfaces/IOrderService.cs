using DemoProject.Domain.DTOs;
using DemoProject.Domain.Entities;
using System.Threading.Tasks;


namespace DemoProject.Domain.Interfaces
{
    public interface IOrderService
    {
        Task<List<OrderDto>> GetOrdersByNameAsync(string? name, CancellationToken ct);
        Task<BaseResponseDTO<Order>> CheckoutAsync(string orderId, CancellationToken ct);
    }
}
