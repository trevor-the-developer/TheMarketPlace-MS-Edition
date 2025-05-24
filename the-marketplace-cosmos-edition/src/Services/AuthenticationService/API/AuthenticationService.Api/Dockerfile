FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/AuthenticationService/API/AuthenticationService.Api/AuthenticationService.Api.csproj", "src/Services/AuthenticationService/API/AuthenticationService.Api/"]
COPY ["src/Services/AuthenticationService/Core/AuthenticationService.Application/AuthenticationService.Application.csproj", "src/Services/AuthenticationService/Core/AuthenticationService.Application/"]
COPY ["src/Services/AuthenticationService/Core/AuthenticationService.Domain/AuthenticationService.Domain.csproj", "src/Services/AuthenticationService/Core/AuthenticationService.Domain/"]
COPY ["src/Services/AuthenticationService/Infrastructure/AuthenticationService.Persistence/AuthenticationService.Persistence.csproj", "src/Services/AuthenticationService/Infrastructure/AuthenticationService.Persistence/"]
COPY ["src/Shared/Services.Core/Services.Core.csproj", "src/Shared/Services.Core/"]
RUN dotnet restore "src/Services/AuthenticationService/API/AuthenticationService.Api/AuthenticationService.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/AuthenticationService/API/AuthenticationService.Api"
RUN dotnet build "AuthenticationService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AuthenticationService.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuthenticationService.Api.dll"]