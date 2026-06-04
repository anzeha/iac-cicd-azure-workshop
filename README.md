# NLB Workshop: Azure IaC, AKS, and Azure DevOps

This repository is the workshop baseline for two connected topics:

- Infrastructure as Code with Bicep
- CI/CD with Azure DevOps and AKS

The app itself is still the same event-driven sample from the previous workshop:

- `Nlb.Workshop.Api` publishes order events
- `Nlb.Workshop.Consumer.Worker` projects them into a read model
- `Nlb.Workshop.Tools.Replay` rebuilds projections from the event stream

What changed is the delivery model:

- local mode now uses Docker compose with SQL Server so it matches the production database engine
- Azure mode uses Azure SQL, Event Hubs, Blob checkpoints, AKS, Helm, and Azure DevOps

## Repo Layout

```text
Nlb.Workshop.Api/                  Minimal API producer
Nlb.Workshop.Consumer.Worker/      Background consumer
Nlb.Workshop.Application/          Use cases and ports
Nlb.Workshop.Domain/               Read-model entities
Nlb.Workshop.Contracts/            API and event contracts
Nlb.Workshop.Infrastructure/       EF Core, messaging, validation, health checks
Nlb.Workshop.Tools.Replay/         Replay utility
infra/                             Bicep entrypoints and modules
helm/                              AKS deployment chart
pipelines/                         Azure DevOps templates
docs/                              Workshop docs and runbooks
compose.yaml                       Local infrastructure
```

## Local Runtime

Local development keeps the workshop emulator story:

- Event Hubs emulator
- Azurite
- Kafka UI
- SQL Server

See [docs/run-local.md](docs/run-local.md).

## Azure Runtime

Azure deployment uses:

- Azure Resource Groups
- Azure Container Registry
- Azure Kubernetes Service
- Azure Key Vault
- Azure SQL Database
- Azure Event Hubs
- Azure Storage account with Blob + Queue
- Azure Service Bus
- Azure DevOps CI and CD pipelines

See:

- [WORKSHOP.md](WORKSHOP.md)
- [docs/workshop-outline.md](docs/workshop-outline.md)
- [docs/architecture-azure.md](docs/architecture-azure.md)
- [docs/run-azure.md](docs/run-azure.md)
- [docs/azure-devops-setup.md](docs/azure-devops-setup.md)

## Main Endpoints

- `POST /orders`
- `POST /orders/bulk`
- `GET /read-model/orders/{orderId}`
- `GET /health`
- `GET /health/live`
- `GET /health/ready`

## Important Implementation Notes

- The read model now uses SQL Server in every environment.
- Local mode uses a Dockerized SQL Server instance.
- Azure mode uses Azure SQL with the same EF Core migration path.
- Runtime secrets are expected to come from Key Vault through AKS Secret Store CSI integration.

## Build

```bash
dotnet restore nlb-workshop-azure.slnx
dotnet build nlb-workshop-azure.slnx
```

## Workshop Sessions

1. Session 1: Bicep and Azure resource provisioning
2. Session 2: Helm, AKS, and Azure DevOps CI/CD

Dodamo
