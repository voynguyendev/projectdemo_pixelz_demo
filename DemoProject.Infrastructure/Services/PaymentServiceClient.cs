using DemoProject.Domain.DTOs;
using DemoProject.Domain.Entities;
using DemoProject.Domain.Helpers;
using DemoProject.Domain.Interfaces;
using DemoProject.GrpcServer.PaymentService;
using Microsoft.Extensions.Logging;


namespace DemoProject.Infrastructure.Services.Mock
{
    public class PaymentServiceClient(ILogger<PaymentServiceClient> logger, PaymentService.PaymentServiceClient  client) : IPaymentServiceClient
    {
        public async Task<BaseResponseDTO> ProcessPaymentAsync(Order order, CancellationToken ct)
        {
            logger.LogInformation($"[{nameof(PaymentServiceClient)}.{nameof(ProcessPaymentAsync)}] Start Process Payment for order: {order.ToSerializeJson()} ");

            var request = new ProcessPaymentRequest
            {
                OrderId = order.Id.ToString(),
            };

            var resultProcessPayment = await client.ProcessPaymentAsync(request);
            var result = new BaseResponseDTO { IsSuccess = true, Message = resultProcessPayment.Message };

            logger.LogInformation($"[{nameof(PaymentServiceClient)}.{nameof(ProcessPaymentAsync)}] End Process Payment for order:{order.ToSerializeJson()},result:{result.ToSerializeJson()}");
            return result;
        }
    }

}
