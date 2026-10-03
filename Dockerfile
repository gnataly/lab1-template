# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Восстановление с кэшем по зависимостям
COPY PersonService/PersonService.csproj PersonService/
RUN dotnet restore PersonService/PersonService.csproj

COPY . .
WORKDIR /src/PersonService
RUN dotnet publish -c Release -o /app/publish

# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV PORT=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "PersonService.dll"]