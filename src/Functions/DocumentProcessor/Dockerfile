FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5003

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Functions/DocumentProcessor/DocumentProcessor.csproj", "src/Functions/DocumentProcessor/"]
COPY ["src/Shared/Services.Core/Services.Core.csproj", "src/Shared/Services.Core/"]
RUN dotnet restore "src/Functions/DocumentProcessor/DocumentProcessor.csproj"
COPY . .
WORKDIR "/src/src/Functions/DocumentProcessor"
RUN dotnet build "DocumentProcessor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DocumentProcessor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DocumentProcessor.dll"]