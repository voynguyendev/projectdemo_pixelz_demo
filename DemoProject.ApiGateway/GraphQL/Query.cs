using DemoProject.Domain.DTOs;
using DemoProject.Domain.Entities;
using DemoProject.Domain.Interfaces;

namespace DemoProject.ApiGateway.GraphQL
{
    public class Query
    {
        public async Task<List<OrderDto>> GetOrders([Service] IOrderService orderService, string? name, CancellationToken ct)
        {
            return await orderService.GetOrdersByNameAsync(name, ct);
        }
    }
}
