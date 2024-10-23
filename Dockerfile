# Use the official .NET SDK image as a base image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

# Add curl for healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

ENV READER_IP_ADDRESS=10.10.69.69
ENV HTTP_PORT=3001
ENV ASPNETCORE_URLS=http://+:3001

# Add user permissions for port binding
USER root
RUN apt-get update && apt-get install -y libcap2-bin
RUN setcap CAP_NET_BIND_SERVICE=+eip /usr/share/dotnet/dotnet

EXPOSE 3001
ENTRYPOINT ["dotnet", "CS203X-reader.dll"]