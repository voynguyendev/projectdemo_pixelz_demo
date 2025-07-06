using DemoProject.Application.Services;
using DemoProject.Domain.DTOs;
using DemoProject.Domain.Interfaces;
using DemoProject.GrpcServer.EmailService;
using DemoProject.GrpcServer.PaymentService;
using DemoProject.GrpcServer.ProductService;
using DemoProject.Infrastructure.Data;
using DemoProject.Infrastructure.Repositories;
using DemoProject.Infrastructure.Services;
using DemoProject.Infrastructure.Services.Mock;
using Grpc.Core;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using Serilog.Sinks.PostgreSQL;


var builder = WebApplication.CreateBuilder(args);
// register Serilog from config write log to DB 
builder.Host.UseSerilog((ctx, lc) =>
{
    lc.ReadFrom.Configuration(ctx.Configuration);

    // Add PostgreSQL sink manually
    var columnWriters = new Dictionary<string, ColumnWriterBase>
{
        { "Message", new RenderedMessageColumnWriter() },
        { "Level", new LevelColumnWriter() },
        { "TimeStamp", new TimestampColumnWriter() },
        { "Exception", new ExceptionColumnWriter() },
        { "LogEvent", new LogEventSerializedColumnWriter() },
        { "Properties", new LogEventSerializedColumnWriter() }
};

    lc.WriteTo.PostgreSQL(
        connectionString: ctx.Configuration.GetConnectionString("DefaultConnection"),
        tableName: "Logs",
        columnOptions: columnWriters,
        needAutoCreateTable: true,
        schemaName: "public"
    );
});

var config = builder.Configuration;



// ---- Database ----
builder.Services.AddDbContext<DemoProjectContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---- Register DI For  IRepository  ----
builder.Services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));


//----register ProductionServiceClient  in DI and Add policy Retry   ----

var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()                                       // 5xx, HttpRequestException, etc.
    .Or<RpcException>(ex => ex.StatusCode == StatusCode.Unavailable)
    .WaitAndRetryAsync(
        retryCount: 3,                                                 // retry up to 3 times
        sleepDurationProvider: retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),          // 2, 4, 8 seconds
        onRetry: (outcome, timespan, retryNumber, context) =>
        {
            // Optional: log the retry
            Console.WriteLine($"Retry {retryNumber} after {timespan}: {outcome.Exception?.Message}");
        });
// Add DI client for ProductionService Grpc

builder.Services.AddGrpcClient<ProductionService.ProductionServiceClient>(o =>
{
    o.Address = new Uri(config.GetValue<string>("GrpcServer:ProductService:Address"));
}).AddPolicyHandler(retryPolicy);

// Add DI client for EmailService Grpc
builder.Services.AddGrpcClient<EmailService.EmailServiceClient>(o =>
{
    o.Address = new Uri(config.GetValue<string>("GrpcServer:EmailService:Address"));
}).AddPolicyHandler(retryPolicy);

// Add DI client for PaymentService Grpc
builder.Services.AddGrpcClient<PaymentService.PaymentServiceClient>(o =>
{
    o.Address = new Uri(config.GetValue<string>("GrpcServer:PaymentService:Address"));
}).AddPolicyHandler(retryPolicy);

// ---- Register DI For  Client Service  ----
builder.Services.AddScoped(typeof(IEmailServiceClient), typeof(EmailServiceClient));

builder.Services.AddScoped(typeof(IPaymentServiceClient), typeof(PaymentServiceClient));

builder.Services.AddScoped(typeof(IProductionServiceClient), typeof(ProductionServiceClient));

// ---- Register DI For   Service  ----
builder.Services.AddScoped(typeof(IOrderService), typeof(OrderService));

// ---- Register Hangfire  ----
//For better performance in production applications, we should change the storage to Redis. Please refer to: https://docs.hangfire.io/en/latest/configuration/using-redis.html 
//Push Order to Product Service and Send mail in Queues(Background job) if fail auto retry or manual trigger again
builder.Services.AddHangfire(config =>
          config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
         .UseSimpleAssemblyNameTypeSerializer()
         .UseDefaultTypeSerializer()
         .UseMemoryStorage());

builder.Services.AddHangfireServer();

//Add GraphQL

builder.Services.AddGraphQLServer()
            .AddQueryType<DemoProject.ApiGateway.GraphQL.Query>()
            .AddMutationType<DemoProject.ApiGateway.GraphQL.Mutation>();

var app = builder.Build();

app.MapGraphQL("/graphql");

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();


//auto migration
using (var serviceScope = app.Services.CreateScope())
{
    var context = serviceScope.ServiceProvider.GetRequiredService<DemoProjectContext>();

    if(context.Database.ProviderName!= "Microsoft.EntityFrameworkCore.InMemory")
       context.Database.Migrate();
    var order = context.Orders.FirstOrDefault(x => x.Name == "Order1");
    if (order == null)
    {
        context.Orders.Add(new DemoProject.Domain.Entities.Order
        {
            Id=Guid.Parse("0197deeb-9552-7339-b485-4ee68efaa786"),
            Amount = 1000,
            CreatedAt = DateTime.UtcNow,
            Status = DemoProject.Domain.Enums.EOrderStatus.Created,
            Name = "Order1",
            UpdatedAt = DateTime.UtcNow
        });
        context.SaveChanges();
    }

}



app.Run();

public partial class Program { }

