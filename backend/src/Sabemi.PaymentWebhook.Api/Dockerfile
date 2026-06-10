FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY backend/Sabemi.slnx ./
COPY backend/src/Sabemi.PaymentWebhook.Api/Sabemi.PaymentWebhook.Api.csproj backend/src/Sabemi.PaymentWebhook.Api/
COPY backend/src/Sabemi.PaymentWebhook.Application/Sabemi.PaymentWebhook.Application.csproj backend/src/Sabemi.PaymentWebhook.Application/
COPY backend/src/Sabemi.PaymentWebhook.Domain/Sabemi.PaymentWebhook.Domain.csproj backend/src/Sabemi.PaymentWebhook.Domain/
COPY backend/src/Sabemi.PaymentWebhook.Infrastructure/Sabemi.PaymentWebhook.Infrastructure.csproj backend/src/Sabemi.PaymentWebhook.Infrastructure/

RUN dotnet restore backend/src/Sabemi.PaymentWebhook.Api/Sabemi.PaymentWebhook.Api.csproj

COPY backend backend
RUN dotnet publish backend/src/Sabemi.PaymentWebhook.Api/Sabemi.PaymentWebhook.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Sabemi.PaymentWebhook.Api.dll"]
