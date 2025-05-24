FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/ListingService/API/ListingService.Api/ListingService.Api.csproj", "src/Services/ListingService/API/ListingService.Api/"]
COPY ["src/Services/ListingService/Core/ListingService.Application/ListingService.Application.csproj", "src/Services/ListingService/Core/ListingService.Application/"]
COPY ["src/Services/ListingService/Core/ListingService.Domain/ListingService.Domain.csproj", "src/Services/ListingService/Core/ListingService.Domain/"]
COPY ["src/Services/ListingService/Infrastructure/ListingService.Persistence/ListingService.Persistence.csproj", "src/Services/ListingService/Infrastructure/ListingService.Persistence/"]
COPY ["src/Shared/Services.Core/Services.Core.csproj", "src/Shared/Services.Core/"]
RUN dotnet restore "src/Services/ListingService/API/ListingService.Api/ListingService.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/ListingService/API/ListingService.Api"
RUN dotnet build "ListingService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ListingService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ListingService.Api.dll"]