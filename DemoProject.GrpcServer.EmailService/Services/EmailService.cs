
using Grpc.Core;
using static DemoProject.GrpcServer.EmailService.EmailService;

namespace DemoProject.GrpcServer.EmailService.Services;

public class EmailService : EmailServiceBase
{
    public override Task<SendOrderConfirmationResponse> SendOrderConfirmation(SendOrderConfirmationRequest request, ServerCallContext context)
    {
        return Task.FromResult(new SendOrderConfirmationResponse
        {
            Success = true,
            Message = $"{request.OrderId} Sent Mail Successfully"
        });
    }
}
