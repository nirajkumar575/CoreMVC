# Step 1: Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

# Step 2: Build image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 👉 yaha apna project ka exact .csproj file name likho
COPY ["CoreMVC.csproj", "./"]

RUN dotnet restore "./CoreMVC.csproj"

COPY . .
WORKDIR "/src/."
RUN dotnet build "CoreMVC.csproj" -c Release -o /app/build

# Step 3: Publish
FROM build AS publish
RUN dotnet publish "CoreMVC.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Step 4: Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CoreMVC.dll"]
