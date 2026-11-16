FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY BookingPlatformInsiders.Domain/BookingPlatformInsiders.Domain.csproj BookingPlatformInsiders.Domain/
COPY BookingPlatformInsiders.Application/BookingPlatformInsiders.Application.csproj BookingPlatformInsiders.Application/
COPY BookingPlatformInsiders.Infrastructure/BookingPlatformInsiders.Infrastructure.csproj BookingPlatformInsiders.Infrastructure/
COPY BookingPlatformInsiders/BookingPlatformInsiders.Api.csproj BookingPlatformInsiders/

RUN dotnet restore BookingPlatformInsiders/BookingPlatformInsiders.Api.csproj

COPY . .

RUN dotnet publish BookingPlatformInsiders/BookingPlatformInsiders.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BookingPlatformInsiders.Api.dll"]
