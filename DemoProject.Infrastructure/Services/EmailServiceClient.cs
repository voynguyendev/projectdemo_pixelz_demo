using DemoProject.Domain.DTOs;
using DemoProject.Domain.Entities;
using DemoProject.Domain.Helpers;
using DemoProject.Domain.Interfaces;
using DemoProject.GrpcServer.EmailService;

using Microsoft.Extensions.Logging;


namespace DemoProject.Infrastructure.Services.Mock
{
    public class EmailServiceClient(ILogger<EmailServiceClient> logger, EmailService.EmailServiceClient client) : IEmailServiceClient
    {
        public async Task<BaseResponseDTO> SendOrderConfirmationAsync(Order order, CancellationToken ct)
        {

            //We will not use try-catch because Hangfire will automatically retry if an exception occurs.
            //The Hangfire dashboard allows us to check and manually re-trigger jobs if an exception occurs.
            logger.LogInformation($"[{nameof(EmailServiceClient)}.{nameof(SendOrderConfirmationAsync)}] Start send mail for checkout order: {order.ToSerializeJson()} ");

            var request = new SendOrderConfirmationRequest
            {
                OrderId = order.Id.ToString(),
            };

            var resultSendOrderConfirmation = await client.SendOrderConfirmationAsync(request);
            var result = new BaseResponseDTO { IsSuccess = true, Message = resultSendOrderConfirmation.Message };


            logger.LogInformation($"[{nameof(EmailServiceClient)}.{nameof(SendOrderConfirmationAsync)}] End send mail for checkout order: {order.ToSerializeJson()} ");

            return result;
        }
    }
}
