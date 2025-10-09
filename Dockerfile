# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution file and all project files
COPY *.sln ./
COPY src/MathRacerAPI.Domain/*.csproj ./src/MathRacerAPI.Domain/
COPY src/MathRacerAPI.Infrastructure/*.csproj ./src/MathRacerAPI.Infrastructure/
COPY src/MathRacerAPI.Presentation/*.csproj ./src/MathRacerAPI.Presentation/
COPY tests/MathRacerAPI.Tests/*.csproj ./tests/MathRacerAPI.Tests/

# Restore dependencies
RUN dotnet restore

# Copy the rest of the source code
COPY src/ ./src/
COPY tests/ ./tests/

# Build and publish the presentation project
RUN dotnet publish src/MathRacerAPI.Presentation/MathRacerAPI.Presentation.csproj -c Release -o out

# Use the official .NET 8 runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /app/out .

# Expose the port that the application will run on
EXPOSE 8080

# Set environment variables for production
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Start the application
ENTRYPOINT ["dotnet", "MathRacerAPI.Presentation.dll"]