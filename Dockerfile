# Use the official .NET SDK image as a base image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

# Set the working directory in the container
WORKDIR /app

# Copy everything to the container
COPY . ./

# Restore dependencies
RUN dotnet restore

# Build the application
RUN dotnet publish -c Release -o out

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

ENV READER_IP_ADDRESS=10.10.1.230
ENV HTTP_PORT=3001

# Expose the port the app runs on
EXPOSE 3001

# Set the entry point for the application
ENTRYPOINT ["dotnet", "CS203X-reader.dll"]