# Use .NET 9 SDK image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

# Copy solution and restore
COPY . .
RUN dotnet restore "./CoreMVC.sln"

# Publish app
RUN dotnet publish "./CoreMVC.sln" -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CoreMVC.dll"]
