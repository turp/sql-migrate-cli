#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["cli/cli.csproj", "cli/"]
RUN dotnet restore "cli/cli.csproj"
COPY . .
WORKDIR "/src/cli"
RUN dotnet build "cli.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "cli.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "db-deploy-cli.dll"]