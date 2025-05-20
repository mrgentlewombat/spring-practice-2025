FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY SPP.MasterNode/SPP.MasterNode.csproj SPP.MasterNode/
COPY ["Libraries/SPP.Domain/SPP.Domain.csproj", "Libraries/SPP.Domain/"]
COPY ["Libraries/SPP.Communication/SPP.Communication.csproj", "Libraries/SPP.Communication/"]

RUN dotnet restore SPP.MasterNode/SPP.MasterNode.csproj

COPY . .
WORKDIR /src/SPP.MasterNode
RUN dotnet build SPP.MasterNode.csproj -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish SPP.MasterNode.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SPP.MasterNode.dll"]
