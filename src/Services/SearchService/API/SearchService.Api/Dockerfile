FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/SearchService/API/SearchService.Api/SearchService.Api.csproj", "src/Services/SearchService/API/SearchService.Api/"]
COPY ["src/Services/SearchService/Core/SearchService.Application/SearchService.Application.csproj", "src/Services/SearchService/Core/SearchService.Application/"]
COPY ["src/Services/SearchService/Infrastructure/SearchService.Persistence/SearchService.Persistence.csproj", "src/Services/SearchService/Infrastructure/SearchService.Persistence/"]
COPY ["src/Shared/Services.Core/Services.Core.csproj", "src/Shared/Services.Core/"]
RUN dotnet restore "src/Services/SearchService/API/SearchService.Api/SearchService.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/SearchService/API/SearchService.Api"
RUN dotnet build "SearchService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SearchService.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SearchService.Api.dll"]