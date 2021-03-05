#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0-buster-slim AS base
ENV http_proxy=http://proxy-chain.intel.com:911
ENV http_proxys=http://proxy-chain.intel.com:911
RUN apt-get update
RUN apt-get install -y curl
RUN curl -o /usr/local/share/ca-certificates/IntelSHA256RootCA.crt http://certificates.intel.com/repository/certificates/IntelSHA256RootCA-Base64.crt
RUN curl -o /usr/local/share/ca-certificates/IntelRootCA.crt http://certificates.intel.com/repository/certificates/Intel%20Root%20CA%20Base64.crt
RUN update-ca-certificates
# needed to resolve SSL handshake issue with SQL Server
RUN sed -i 's/DEFAULT@SECLEVEL=2/DEFAULT@SECLEVEL=1/g' /etc/ssl/openssl.cnf

WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
ENV http_proxy=http://proxy-chain.intel.com:911
ENV http_proxys=http://proxy-chain.intel.com:911

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