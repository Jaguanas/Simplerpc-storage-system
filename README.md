# A simpleRPC program

A distributed .NET application demonstrating RPC communication between a
Storage server and two clients: Baker and Farmer.

## Architecture

The solution contains:
- `Storage` - RPC server that stores grain and manages seasons.
- `Baker` - client that consumes grain.
- `Farmer` - client that deposits grain.
- `StorageContract` - shared RPC interfaces and data models.

The applications communicate through SimpleRpc over HTTP.

## Requirements

- .NET 10 SDK
- Docker Desktop
- Docker Compose

## Run Locally

Build the solution:
```bash
dotnet build simplerpc.sln
```

Start the server
```bash
dotnet run --project Storage.csproj
```

In separate terminals, start the clients:
```bash
dotnet run --project Baker.csproj
dotnet run --project Farmer.csproj
```

The default RPC endpoint is:
```bash
http://127.0.0.1:5000/simplerpc
```

## Run with Docker Compose

Start all services:
```bash
docker compose up --build -d
```

View logs:
```bash
docker compose logs -f
```

Stop the services:
```bash
docker compose down
```

Inside the Docker network, clients connect to Storage using:
```bash
http://storage:5000/simplerpc
```

## Project Status

Currently supported:
- Native .NET execution
- Individual Docker images
- Docker network communication
- Docker Compose orchestration
- Publish images to Docker Hub
- Deploy to Kubernetes
- Manage Kubernetes resources with Terraform

## License

This project is for educational purposes.
