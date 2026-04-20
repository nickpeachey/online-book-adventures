# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/OnlineBookAdventures.Api/OnlineBookAdventures.Api.csproj", "src/OnlineBookAdventures.Api/"]
COPY ["src/OnlineBookAdventures.Application/OnlineBookAdventures.Application.csproj", "src/OnlineBookAdventures.Application/"]
COPY ["src/OnlineBookAdventures.Domain/OnlineBookAdventures.Domain.csproj", "src/OnlineBookAdventures.Domain/"]
COPY ["src/OnlineBookAdventures.Infrastructure/OnlineBookAdventures.Infrastructure.csproj", "src/OnlineBookAdventures.Infrastructure/"]

RUN dotnet restore "src/OnlineBookAdventures.Api/OnlineBookAdventures.Api.csproj"

COPY . .
RUN dotnet publish "src/OnlineBookAdventures.Api/OnlineBookAdventures.Api.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OnlineBookAdventures.Api.dll"]
