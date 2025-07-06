
using DemoProject.Domain.DTOs;
using DemoProject.Domain.Entities;
using DemoProject.Domain.Enums;
using DemoProject.Domain.Helpers;
using DemoProject.Domain.Interfaces;
using DemoProject.Infrastructure.Data;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace DemoProject.IntegrationTests
{
    
    public class DemoProjectTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private  WebApplicationFactory<Program> _factory;
        private  Mock<IPaymentServiceClient> _paymentServiceClientMock = new();
        private  Mock<IEmailServiceClient> _emailServiceClientMock = new();
        private  Mock<IProductionServiceClient> _productionServiceClientMock = new();
       
        public DemoProjectTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {

                    services.RemoveAll<IProductionServiceClient>();
                    services.AddSingleton(_productionServiceClientMock.Object);
                    services.RemoveAll<IEmailServiceClient>();
                    services.AddSingleton(_paymentServiceClientMock.Object);
                    services.RemoveAll<IEmailServiceClient>();
                    services.AddSingleton(_emailServiceClientMock.Object);

                    // If we are using a real database, we might switch to an in-memory one for tests.
                    // But note: the API Gateway might not have a direct database dependency? 
                  

                    services.Remove
                    (
                        services.SingleOrDefault(d => d.ServiceType == typeof(IDbContextOptionsConfiguration<DemoProjectContext>))
                    );

                    services.AddDbContext<DemoProjectContext>(options =>
                              options.UseInMemoryDatabase("pixelz_demo"));
                });
            });
        }

        [Fact]
        public async Task Checkout_PaymentSuccess_PushProductServerSuccess_ValidOrder_ShouldReturnSuccess()
        {

            var client = _factory.CreateClient();
            var graphQLClient = new GraphQLHttpClient(new GraphQLHttpClientOptions
            {
                EndPoint = new Uri("http://localhost/graphql")
            }, new NewtonsoftJsonSerializer(), client);
            // Seed the database with an order
            var orderId = Guid.NewGuid();
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DemoProjectContext>();
            dbContext.Orders.Add(new Order { Id = orderId, Name = "Test Order1", Amount = 100.0m, Status = EOrderStatus.Created });
            await dbContext.SaveChangesAsync();

            // Setup the mocks
            _paymentServiceClientMock
                .Setup(p => p.ProcessPaymentAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseResponseDTO { IsSuccess = true });

            
            _emailServiceClientMock
                .Setup(p => p.SendOrderConfirmationAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseResponseDTO { IsSuccess = true });
           
            _productionServiceClientMock
                .Setup(p => p.PushOrderAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseResponseDTO { IsSuccess = true });
            // Act
           

            var request = new GraphQL.GraphQLRequest
            {
                Query = @"
                mutation CheckoutOrder($orderId: String!) {
                    checkoutOrder(orderId: $orderId) {
                        isSuccess
                        message
                    }
                }
            ",
                Variables = new
                {
                    orderId = orderId.ToString(),
                   
                }
            };

            var response = await graphQLClient.SendMutationWithCorrectResponseAsync<BaseResponseDTO>(request);

            // Assert
            Assert.Equal(true, response.IsSuccess);

            // Verify the order status was updated in the database
            var updatedOrder = await dbContext.Orders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == orderId);

            Assert.Equal(EOrderStatus.Complete, updatedOrder.Status);

            // Verify the invoice created in  the database
            var invoiceOrder = await dbContext.Invoices.AsNoTracking().FirstOrDefaultAsync(x => x.OrderId == orderId);
            Assert.NotNull(invoiceOrder);
        }

        [Fact]
        public async Task Checkout_PaymentFail_ValidOrder_ShouldReturnFail()
        {

            // Arrange
            var client = _factory.CreateClient();
            var graphQLClient = new GraphQLHttpClient(new GraphQLHttpClientOptions
            {
                EndPoint = new Uri("http://localhost/graphql")
            }, new NewtonsoftJsonSerializer(), client);
            // Seed the database with an order
            var orderId = Guid.NewGuid();
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DemoProjectContext>();
            dbContext.Orders.Add(new Order { Id = orderId, Name = "Test Order2", Amount = 100.0m, Status = EOrderStatus.Created });
            await dbContext.SaveChangesAsync();


            // Setup the mocks
            _paymentServiceClientMock
                 .Setup(p => p.ProcessPaymentAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new BaseResponseDTO { IsSuccess = false });


            // Act

            var request = new GraphQL.GraphQLRequest
            {
                Query = @"
                mutation CheckoutOrder($orderId: String!) {
                    checkoutOrder(orderId: $orderId) {
                        isSuccess
                        message
                    }
                }
            ",
                Variables = new
                {
                    orderId = orderId.ToString(),

                }
            };
            var response = await graphQLClient.SendMutationWithCorrectResponseAsync<BaseResponseDTO>(request);

            // Assert
            Assert.Equal(false, response.IsSuccess);

            // Verify the order status was updated in the database
            var updatedOrder = await dbContext.Orders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == orderId);
            Assert.Equal(EOrderStatus.PaymentFailed, updatedOrder.Status);

        }

        [Fact]
        public async Task Checkout_PaymentSuccess_PushProductServerFail_ValidOrder_ShouldReturnFail()
        {

            // Arrange
            var client = _factory.CreateClient();
            var graphQLClient = new GraphQLHttpClient(new GraphQLHttpClientOptions
            {
                EndPoint = new Uri("http://localhost/graphql")
            }, new NewtonsoftJsonSerializer(), client);
            // Seed the database with an order
            var orderId = Guid.NewGuid();
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DemoProjectContext>();
            dbContext.Orders.Add(new Order { Id = orderId, Name = "Test Order3", Amount = 100.0m, Status = EOrderStatus.Created });
            await dbContext.SaveChangesAsync();


            // Setup the mocks
            _paymentServiceClientMock
              .Setup(p => p.ProcessPaymentAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(new BaseResponseDTO { IsSuccess = true });


            _emailServiceClientMock
                .Setup(p => p.SendOrderConfirmationAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseResponseDTO { IsSuccess = true });

            _productionServiceClientMock
               .Setup(p => p.PushOrderAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new BaseResponseDTO { IsSuccess = false });
            // Act
            // Act

            var request = new GraphQL.GraphQLRequest
            {
                Query = @"
                mutation CheckoutOrder($orderId: String!) {
                    checkoutOrder(orderId: $orderId) {
                        isSuccess
                        message
                    }
                }
            ",
                Variables = new
                {
                    orderId = orderId.ToString(),

                }
            };
            var response = await graphQLClient.SendMutationWithCorrectResponseAsync<BaseResponseDTO>(request);

            // Assert
            Assert.Equal(false, response.IsSuccess);

            // Verify the invoice created in  the database
            var invoiceOrder = await dbContext.Invoices.AsNoTracking().FirstOrDefaultAsync(x => x.OrderId == orderId);
            Assert.NotNull(invoiceOrder);

            // Verify the order status was updated in the database
            var updatedOrder = await dbContext.Orders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == orderId);
            Assert.Equal(EOrderStatus.PushOrderFailed, updatedOrder.Status);

        }

        [Fact]
        public async Task SearchOrderByName_ShouldFoundOrders()
        {
            // Arrange
            var client = _factory.CreateClient();
            var graphQLClient = new GraphQLHttpClient(new GraphQLHttpClientOptions
            {
                EndPoint = new Uri("http://localhost/graphql")
            }, new NewtonsoftJsonSerializer(), client);
            // Seed the database with an order
           
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DemoProjectContext>();

            dbContext.Orders.Add(new Order { Id = Guid.NewGuid(), Name = "Test Order 1", Amount = 100.0m, Status = EOrderStatus.Created });
            dbContext.Orders.Add(new Order { Id = Guid.NewGuid(), Name = "Test Order 2", Amount = 100.0m, Status = EOrderStatus.Created });
            await dbContext.SaveChangesAsync();



            var request = new GraphQL.GraphQLRequest
            {
                Query = @"
                query Orders($name: String!) {
                    orders(name: $name) {
                         id
                         name
                         status
                    }
                }
            ",
                Variables = new
                {
                    name = "Test Order 1",

                }
            };
            var response = await graphQLClient.SendQueryWithCorrectResponseAsync<List<OrderDto>>(request);

            // Assert
            Assert.Equal(1, response.Count);
            Assert.Equal("Test Order 1", response[0].Name);
        }
        
        [Fact]
        public async Task SearchOrderByName_ShouldNotFoundOrders()
        {
            // Arrange
            var client = _factory.CreateClient();
            var graphQLClient = new GraphQLHttpClient(new GraphQLHttpClientOptions
            {
                EndPoint = new Uri("http://localhost/graphql")
            }, new NewtonsoftJsonSerializer(), client);
            // Seed the database with an order

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DemoProjectContext>();
            dbContext.Orders.Add(new Order { Id = Guid.NewGuid(), Name = "Test Order 3", Amount = 100.0m, Status = EOrderStatus.Created });
            dbContext.Orders.Add(new Order { Id = Guid.NewGuid(), Name = "Test Order 4", Amount = 100.0m, Status = EOrderStatus.Created });
            await dbContext.SaveChangesAsync();


            var request = new GraphQL.GraphQLRequest
            {
                Query = @"
                query Orders($name: String!) {
                    orders(name: $name) {
                         id
                         name
                         status
                    }
                }
            ",
                Variables = new
                {
                    name = "Test Order 5",

                }
            };
            var response = await graphQLClient.SendQueryWithCorrectResponseAsync<List<OrderDto>>(request);

            // Assert
            Assert.Equal(0, response.Count);

        }

    }
}
