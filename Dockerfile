FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy everything
COPY FuturesPriceComparison.sln ./
COPY **/*.csproj ./FuturesPriceComparison.PriceChecker/
# Restore as distinct layers
RUN dotnet restore
COPY ./ ./
# Build and publish a release
RUN dotnet publish -c Release -o out

FROM base AS final
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "FuturesPriceComparison.PriceChecker.dll"]
