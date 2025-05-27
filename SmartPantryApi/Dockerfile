# Base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["SmartPantryApi.csproj", ""]
RUN dotnet restore "./SmartPantryApi.csproj"

COPY . .
WORKDIR "/src/SmartPantryApi"
RUN dotnet publish "./SmartPantryApi.csproj" -c Release -o /app/publish

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SmartPantryApi.dll"]