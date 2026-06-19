FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8085
EXPOSE 8086

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["CifraShop.API/CifraShop.API.csproj", "CifraShop.API/"]
COPY ["CifraShop.Application/CifraShop.Application.csproj", "CifraShop.Application/"]
COPY ["CifraShop.Contracts/CifraShop.Contracts.csproj", "CifraShop.Contracts/"]
COPY ["CifraShop.Domain/CifraShop.Domain.csproj", "CifraShop.Domain/"]
COPY ["CifraShop.Infrastructure/CifraShop.Infrastructure.csproj", "CifraShop.Infrastructure/"]
RUN dotnet restore "./CifraShop.API/CifraShop.API.csproj"

COPY . .
WORKDIR "/src/CifraShop.API"
RUN dotnet build "./CifraShop.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CifraShop.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CifraShop.API.dll"]
