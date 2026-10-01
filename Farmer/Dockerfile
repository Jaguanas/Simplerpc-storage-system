FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Farmer/Farmer.csproj Farmer/
COPY StorageContract/StorageContract.csproj StorageContract/
RUN dotnet restore Farmer/Farmer.csproj

COPY . .
RUN dotnet publish Farmer/Farmer.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app

COPY --from=build app/publish .
EXPOSE 5000

ENTRYPOINT [ "dotnet", "Farmer.dll" ]