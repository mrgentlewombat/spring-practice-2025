# build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# copy entire project 
COPY . .

# Restore dependencies
RUN dotnet restore "./WorkerNodeApp.csproj"

# App compile
RUN dotnet build "./WorkerNodeApp.csproj" -c Release -o /app/build


FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app
COPY --from=build /app/build .

# Run app
ENTRYPOINT ["dotnet", "WorkerNodeApp.dll"]
