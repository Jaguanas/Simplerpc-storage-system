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

## Run with Kubernetes

Create local cluster:
```bash
kind create cluster --name simplerpc
```

Create the namespace:
```bash
kubectl apply -f namespace.yaml
```

Apply the storage server and service:
```bash
kubectl apply -f storage.yaml
```

Apply the clients:
```bash
kubectl apply -f baker.yaml
kubectl apply -f farmer.yaml
```

View logs:
```bash
kubectl logs -f deployment/storage -n simplerpc
kubectl logs -f deployment/baker -n simplerpc
kubectl logs -f deployment/farmer -n simplerpc
```

Remove the application namespace:
```bash
kubectl delete namespace simplerpc
```

Delete the local kind cluster:
```bash
kind delete cluster --name simplerpc
```

## Run with Terraform

If kind cluster doesn't exists, create it:
```bash
kind create cluster --name simplerpc
```

Open the Terraform folder:
```bash
cd .\terraform
```

Initialize Terraform
```bash
terraform init
```

Format and validate
```bash
terraform fmt
terraform validate
```

Preview changes:
```bash
terraform plan
```

Apply configuration:
```bash
terraform apply
```

Check Kubernetes resources:
```bash
kubectl get all -n simplerpc-tf
```

View application logs:
```bash
kubectl logs -f deployment/storage -n simplerpc-tf
kubectl logs -f deployment/baker -n simplerpc-tf
kubectl logs -f deployment/farmer -n simplerpc-tf
```

To stop the Terraform-managed application:
```bash
terraform destroy
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
