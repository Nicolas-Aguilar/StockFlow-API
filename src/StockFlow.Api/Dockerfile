# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
WORKDIR /src

COPY StockFlow.sln ./
COPY src/StockFlow.Api/StockFlow.Api.csproj src/StockFlow.Api/
COPY src/StockFlow.Application/StockFlow.Application.csproj src/StockFlow.Application/
COPY src/StockFlow.Domain/StockFlow.Domain.csproj src/StockFlow.Domain/
COPY src/StockFlow.Infrastructure/StockFlow.Infrastructure.csproj src/StockFlow.Infrastructure/

RUN dotnet restore "StockFlow.sln"

FROM restore AS build
COPY . .
RUN dotnet build "StockFlow.sln" -c Release --no-restore

FROM build AS publish
RUN dotnet publish "src/StockFlow.Api/StockFlow.Api.csproj" -c Release -o /app/publish --no-build /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "StockFlow.Api.dll"]
