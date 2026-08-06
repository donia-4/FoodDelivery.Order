# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

COPY Order.API/*.csproj Order.API/
COPY Order.Application/*.csproj Order.Application/
COPY Order.Domain/*.csproj Order.Domain/
COPY Order.Infrastructure/*.csproj Order.Infrastructure/

RUN dotnet restore Order.API/Order.API.csproj

COPY . .

WORKDIR /source/Order.API
RUN dotnet publish -c Release -o /app --no-restore

# Final Stage: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Order.API.dll"]