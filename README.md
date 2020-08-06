# Avanti Platform - Sample Order Warehouse Service

A example service for processing orders and send them to warehouses. This service reacts to incoming OrderCreated events and then retrieves order and product data. Splits the order into seperate warehouse orders and sends them.

This service is an example of processing incoming events and a high performant non-blocking processing pipeline.

## Prerequisites

- Windows / Linux / MacOS
- ASP.NET Core 3.1 Runtime
- PostgreSQL 12 or higher

## Installation

```bash
dotnet restore
```

**For local development, some settings have to be set before the service can function. These settings are set using
environment variables. To start, copy the `.env.example` to `.env` and alter the file for your needs. Without this
step, the service will probably not work!**

## Starting the service

The service needs a datastore to store it's data. For local development, use a local PostgreSQL instance. Recommended to use the Docker version:

```bash
docker run -e POSTGRES_PASSWORD=Passw0rd -p 5432:5432 -d postgres:12
```

It could also need a Azure Storage Account to store blobs and event support (if the service implements it). There is an open-source emulator available which can be started with:

```bash
docker run -d -p 10000:10000 -p 10001:10001 mcr.microsoft.com/azure-storage/azurite
```

The service can be started after PostgreSQL and Azurite are running and the '.env' is available. Start the service:

```bash
dotnet run -p ./src/Avanti.Service.ProcessingService/
```

### Starting a localhost cluster

Start the first service as described above (will start on port 2552 as default, see `avanti-service.yaml`). Open a second terminal and set the following:

```bash
export Main__HttpPort=5001
export Main__ClusterPort=2553
export AkkaSettings__ClusterDiscoveryResolver=EnvironmentVariables
export EnvVarsAkkaClusterSettings__ClusterSeeds=localhost:2552
```

Then start the service as above.

### Starting a cluster outside Kubernetes on multiple hosts

On each host, start the service using the following settings:

```bash
export AkkaSettings__ClusterDiscoveryResolver=EnvironmentVariables
export EnvVarsAkkaClusterSettings__UseHostIpAsPublic=true
export EnvVarsAkkaClusterSettings__ClusterSeeds=<OtherIP>:2552
```

Then start the service as above.

## Testing

```bash
dotnet test ./test/ProcessingServiceTests/
```

# API

This service has Swagger integration for querying available endpoints. The Swagger UI is available on `/swagger` of the HTTP port (5000) for the service.
