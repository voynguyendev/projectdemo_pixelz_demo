using DemoProject.Domain.DTOs;
using DemoProject.Domain.Entities;
using DemoProject.Domain.Helpers;
using DemoProject.Domain.Interfaces;
using DemoProject.GrpcServer.ProductService;
using DemoProject.Infrastructure.Services.Mock;
using Microsoft.Extensions.Logging;


namespace DemoProject.Infrastructure.Services
{
    public class ProductionServiceClient(ILogger<ProductionServiceClient> logger, ProductionService.ProductionServiceClient client, bool alwaysSucceed = true) : IProductionServiceClient
    {
        public async Task<BaseResponseDTO>
            PushOrderAsync(Order order, CancellationToken ct)
        {

            logger.LogInformation($"[{nameof(ProductionServiceClient)}.{nameof(PushOrderAsync)}] Start Push to  server for order: {order.ToSerializeJson()} ");

            var request = new PushOrderRequest
            {
                OrderId = order.Id.ToString(),
            };

            var resultPushOrder = await client.PushOrderAsync(request);
            var result = new BaseResponseDTO { IsSuccess = true, Message = resultPushOrder.Message };

            logger.LogInformation($"[{nameof(ProductionServiceClient)}.{nameof(PushOrderAsync)}] End Push to  server for order: {order.ToSerializeJson()},result:{result.ToSerializeJson()} ");

            return result;
        }
    }
}
