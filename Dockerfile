# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["BotanicalBuddy.API/BotanicalBuddy.API.csproj", "BotanicalBuddy.API/"]
RUN dotnet restore "BotanicalBuddy.API/BotanicalBuddy.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/BotanicalBuddy.API"
RUN dotnet build "BotanicalBuddy.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "BotanicalBuddy.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published files
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "BotanicalBuddy.API.dll"]
