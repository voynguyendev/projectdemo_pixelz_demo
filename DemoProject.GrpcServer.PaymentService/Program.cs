//using DemoProject.GrpcServer.PaymentService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapGrpcService<DemoProject.GrpcServer.PaymentService.Services.PaymentService>();
app.MapGet("/", () => "Payment Server Grpc for Demo");

app.Run();
