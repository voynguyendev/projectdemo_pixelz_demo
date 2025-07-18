using DemoProject.GrpcServer.ProductService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapGrpcService<ProductService>();
app.MapGet("/", () => "Product Server Grpc for Demo");

app.Run();
