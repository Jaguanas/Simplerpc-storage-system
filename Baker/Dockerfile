FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Baker/Baker.csproj Baker/
COPY StorageContract/StorageContract.csproj StorageContract/
RUN dotnet restore Baker/Baker.csproj

COPY . .
RUN dotnet publish Baker/Baker.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app

COPY --from=build app/publish .
EXPOSE 5000

ENTRYPOINT [ "dotnet", "Baker.dll" ]