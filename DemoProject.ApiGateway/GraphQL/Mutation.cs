using DemoProject.Domain.DTOs;
using DemoProject.Domain.Entities;
using DemoProject.Domain.Interfaces;

namespace DemoProject.ApiGateway.GraphQL
{
    public class Mutation
    {
        public async Task<BaseResponseDTO<Order>> CheckoutOrder([Service] IOrderService orderService, string orderId, CancellationToken ct)
        {
            var result = await orderService.CheckoutAsync(orderId, ct);
            return result;
        }
    }
}
