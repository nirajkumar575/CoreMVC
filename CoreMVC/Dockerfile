# -------- Build stage --------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and restore as distinct layers
COPY . .
RUN dotnet restore "./CoreMVC.sln"

# Publish the app
RUN dotnet publish "./CoreMVC/CoreMVC.csproj" -c Release -o /app/publish

# -------- Runtime stage --------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Render sets PORT env var, use it for Kestrel
CMD ["bash","-lc","ASPNETCORE_URLS=http://0.0.0.0:$PORT dotnet CoreMVC.dll"]
