FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything from repository root
COPY . .

# Restore
RUN dotnet restore Services/OrderService/OrderService.Api/OrderService.Api.csproj

# Publish
RUN dotnet publish Services/OrderService/OrderService.Api/OrderService.Api.csproj \
    -c Release \
    -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8081

ENV ASPNETCORE_URLS=http://+:8081

ENTRYPOINT ["dotnet", "OrderService.Api.dll"]