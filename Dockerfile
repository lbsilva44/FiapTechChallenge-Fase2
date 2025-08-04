# Etapa base (runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Configura ASP.NET para ouvir em todas as interfaces na porta 80
ENV ASPNETCORE_URLS=http://+:80

EXPOSE 80

# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

WORKDIR /src/Fiap.Cloud.Games.API

RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Etapa final
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Fiap.Cloud.Games.API.dll"]