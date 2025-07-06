using Grpc.Core;

namespace DemoProject.GrpcServer.ProductService.Services;
public class ProductService : ProductionService.ProductionServiceBase
{
    public override Task<PushOrderResponse> PushOrder(PushOrderRequest request, ServerCallContext context)
    {
        return Task.FromResult(new PushOrderResponse
        {
            Success = true,
            Message= $"{request.OrderId} Pushed to Product Server"
        });
    }
}
