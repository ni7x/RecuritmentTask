# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy the solution file and project files
COPY RecuritmentTask.sln .  
COPY RecuritmentTask/src/RecuritmentTask/RecuritmentTask.csproj ./RecuritmentTask/src/RecuritmentTask/RecuritmentTask/ 
COPY RecuritmentTask/test/RecuritmentTaskTests/RecuritmentTaskTests.csproj ./RecuritmentTask/test/RecuritmentTaskTests/  

# Copy the rest of the application files
COPY RecuritmentTask/ ./RecuritmentTask/ 

# Restore dependencies
RUN dotnet restore RecuritmentTask.sln 

# Build the application
RUN dotnet build RecuritmentTask/src/RecuritmentTask/RecuritmentTask.csproj  -c Release -o /app/build

# Run tests
RUN dotnet test

# Publish the application
RUN dotnet publish RecuritmentTask/src/RecuritmentTask/RecuritmentTask.csproj  -c Release -o /app/publish

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
EXPOSE 8080
WORKDIR /app

# Copy the published app from the build stage
COPY --from=build-env /app/publish .

# Set the entrypoint
ENTRYPOINT ["dotnet", "RecuritmentTask.dll"]