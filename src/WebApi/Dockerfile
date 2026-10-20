# Build context: repository root
# docker build -f src/WebApi/Dockerfile .

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Core/Core.csproj Core/
COPY src/Domain/Domain.csproj Domain/
COPY src/Application/Application.csproj Application/
COPY src/Persistence/Persistence.csproj Persistence/
COPY src/WebApi/WebApi.csproj WebApi/

RUN dotnet restore WebApi/WebApi.csproj

COPY src/Core/ Core/
COPY src/Domain/ Domain/
COPY src/Application/ Application/
COPY src/Persistence/ Persistence/
COPY src/WebApi/ WebApi/

RUN dotnet publish WebApi/WebApi.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# curl for Compose healthcheck against /health
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
RUN mkdir -p /app/wwwroot/uploads/profiles
ENTRYPOINT ["dotnet", "WebApi.dll"]
