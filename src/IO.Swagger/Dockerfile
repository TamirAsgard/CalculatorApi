# Development stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS development
WORKDIR /app
EXPOSE 5000

# Copy csproj and restore dependencies
COPY src/IO.Swagger/*.csproj ./
RUN dotnet restore

# Copy everything else
COPY src/IO.Swagger/ ./

# Build for development
RUN dotnet build -c Debug -o /app/build

# Development runtime with hot reload
ENTRYPOINT ["dotnet", "watch", "run", "--no-restore", "--urls", "http://0.0.0.0:5000"]

# Build stage for production
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY src/IO.Swagger/*.csproj ./IO.Swagger/
RUN dotnet restore "./IO.Swagger/IO.Swagger.csproj"

# Copy source code
COPY src/IO.Swagger/ ./IO.Swagger/

# Build and publish
WORKDIR "/src/IO.Swagger"
RUN dotnet build "IO.Swagger.csproj" -c Release -o /app/build
RUN dotnet publish "IO.Swagger.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Production runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS production
WORKDIR /app
EXPOSE 5001

# Copy published app
COPY --from=build /app/publish .

# Create non-root user for security
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "IO.Swagger.dll"]
