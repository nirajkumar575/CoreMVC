# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["CoreMVC/CoreMVC.csproj", "CoreMVC/"]
RUN dotnet restore "CoreMVC/CoreMVC.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src/CoreMVC"
RUN dotnet build "CoreMVC.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "CoreMVC.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CoreMVC.dll"]
