

using DemoProject.GrpcServer.EmailService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<DemoProject.GrpcServer.EmailService.Services.EmailService>();
app.MapGet("/", () => "Email Server Grpc for Demo");

app.Run();
