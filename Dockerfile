# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY src/Hearty.WebApp/ ./
RUN dotnet restore

# Copy everything else and build
# COPY src/Hearty.WebApp/* ./
RUN dotnet publish -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out ./

# Build uses production environment by default which has a mount point for persistence
RUN mkdir -p /datapersistence

# Expose the port your app runs on
EXPOSE 5030

# Set environment variable for ASP.NET Core to listen on port 5030
ENV ASPNETCORE_URLS=http://+:5030

# Run the app
ENTRYPOINT ["dotnet", "Hearty.WebApp.dll"]
