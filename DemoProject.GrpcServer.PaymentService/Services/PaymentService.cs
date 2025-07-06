using Grpc.Core;
using static DemoProject.GrpcServer.PaymentService.PaymentService;

namespace DemoProject.GrpcServer.PaymentService.Services;
public class PaymentService : PaymentServiceBase
{
    public override Task<ProcessPaymentResponse> ProcessPayment (ProcessPaymentRequest request, ServerCallContext context)
    {
        return Task.FromResult(new ProcessPaymentResponse
        {
            Success = true,
            Message= $"{request.OrderId} Process Payment Successfully "
        });
    }
}
