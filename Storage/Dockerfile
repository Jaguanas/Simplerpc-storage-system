FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Storage/Storage.csproj Storage/
COPY StorageContract/StorageContract.csproj StorageContract/
RUN dotnet restore Storage/Storage.csproj

COPY . .
RUN dotnet publish Storage/Storage.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/publish .
EXPOSE 5000

ENTRYPOINT ["dotnet", "Storage.dll"]