FROM mcr.microsoft.com/dotnet/aspnet:9.0-preview AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY SPP.WorkerNode/SPP.WorkerNode.csproj SPP.WorkerNode/
COPY ["Libraries/SPP.Communication/SPP.Communication.csproj", "Libraries/SPP.Communication/"]

RUN dotnet restore "SPP.WorkerNode/SPP.WorkerNode.csproj"

COPY . .
WORKDIR "/src/SPP.WorkerNode"
RUN dotnet build "SPP.WorkerNode.csproj" -c $BUILD_CONFIGURATION -o /app/build
