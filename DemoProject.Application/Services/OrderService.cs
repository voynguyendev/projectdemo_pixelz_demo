using DemoProject.Domain.DTOs;
using DemoProject.Domain.Entities;
using DemoProject.Domain.Enums;
using DemoProject.Domain.Helpers;
using DemoProject.Domain.Interfaces;
using DemoProject.Infrastructure.Services.Mock;
using Hangfire;
using Microsoft.Extensions.Logging;


namespace DemoProject.Application.Services
{
    public class OrderService(
        IRepository<Order,Guid> orderRepository,
        IRepository<Invoice, Guid> invoiceRepository,
        IEmailServiceClient emailServiceClient,
        IPaymentServiceClient paymentServiceClient,
        IProductionServiceClient productionServiceClient,
        ILogger<OrderService> logger) :IOrderService
    {
        public async Task<List<OrderDto>> GetOrdersByNameAsync(string? name,CancellationToken ct) {

            var queryResult = await orderRepository.GetManyByPredicateAsync(x => string.IsNullOrEmpty(name) || x.Name.ToLower().Contains(name.ToLower()), (query) =>
            {
                return query.OrderByDescending(x => x.CreatedAt);
            }, ct: ct);

            return queryResult.Select(x => new OrderDto
            {
                Amount = x.Amount,
                CreatedAt = x.CreatedAt,
                Id = x.Id,
                Name = x.Name,
                Status = x.Status,
                UpdatedAt = x.UpdatedAt
            }).ToList();


        }
        public async Task<BaseResponseDTO<Order>> CheckoutAsync(string orderId, CancellationToken ct)
        {
            logger.LogInformation($"[{nameof(OrderService)}.{nameof(CheckoutAsync)}] Start Check for order: {orderId} ");

            var order = await orderRepository.GetOneByPredicateAsync(x => x.Id == Guid.Parse(orderId), ct: ct);

            if (order == null)
            {
                logger.LogError($"[{nameof(OrderService)}.{nameof(CheckoutAsync)}] Order not found  : {orderId} ");
                return new BaseResponseDTO<Order> { Message = "Order not found" };
            }

            // Process payment
            var paymentResult = await paymentServiceClient.ProcessPaymentAsync(order, ct);
            if (!paymentResult.IsSuccess)
            {
                order.Status = EOrderStatus.PaymentFailed;
                order.UpdatedAt = DateTime.UtcNow;
                orderRepository.Update(order);
                return new BaseResponseDTO<Order> { IsSuccess = false, Message = paymentResult.Message };
            }

            // Update order status
            order.Status = EOrderStatus.CheckedOut;
            order.UpdatedAt = DateTime.UtcNow;
            orderRepository.Update(order, false);
            //Saved to database
            await orderRepository.SaveChangesAsync(ct);
            //Create new Invoice
            invoiceRepository.Insert(new Invoice
            {
                Amount = order.Amount,
                IssuedAt = DateTime.UtcNow,
                OrderId = Guid.Parse(orderId),
            }, false);


            //Send Mail in Queues
            BackgroundJob.Enqueue(() => emailServiceClient.SendOrderConfirmationAsync(order, ct));

            //in Production we can push in queue
            var resultPushOrder = await productionServiceClient.PushOrderAsync(order, ct);
            if (!resultPushOrder.IsSuccess)
            {
                order.Status = EOrderStatus.PushOrderFailed;
                order.UpdatedAt = DateTime.UtcNow;
                orderRepository.Update(order, false);
                await orderRepository.SaveChangesAsync(ct);
                return new BaseResponseDTO<Order> { IsSuccess = false, Message = resultPushOrder.Message, Data = order };
            }
            order.Status = EOrderStatus.Complete;
            order.UpdatedAt = DateTime.UtcNow;
            orderRepository.Update(order, false);
            await orderRepository.SaveChangesAsync(ct);


            logger.LogInformation($"[{nameof(OrderService)}.{nameof(CheckoutAsync)}] End Check for order: {order.ToSerializeJson()} ");

            return new BaseResponseDTO<Order> { IsSuccess = true, Message = "Checkout successful", Data = order };


        }
    }
}
