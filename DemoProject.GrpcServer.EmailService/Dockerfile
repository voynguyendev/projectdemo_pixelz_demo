# Giai đoạn 1: Build thời gian chạy từ image ASP.NET Core
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 6002
EXPOSE 443

# Khai báo volume để lưu trữ dữ liệu
VOLUME ["/root/.aspnet/DataProtection-Keys"]



# Giai đoạn 2: Build ứng dụng với SDK
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
# Copy file csproj và restore các package
COPY ["DemoProject.GrpcServer.EmailService/DemoProject.GrpcServer.EmailService.csproj", "DemoProject.GrpcServer.EmailService/"]

# Copy toàn bộ mã nguồn vào container
COPY . .
WORKDIR "/src/."

# Build dự án (có thể thay đổi cấu hình nếu cần)
RUN dotnet build "DemoProject.GrpcServer.EmailService/DemoProject.GrpcServer.EmailService.csproj" -c Release -o /app/build

# Giai đoạn 3: Publish ứng dụng
FROM build AS publish
RUN dotnet publish "DemoProject.GrpcServer.EmailService/DemoProject.GrpcServer.EmailService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Giai đoạn 4: Tạo image chạy cuối cùng
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DemoProject.GrpcServer.EmailService.dll"]
