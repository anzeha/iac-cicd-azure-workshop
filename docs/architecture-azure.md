# Azure Architecture

## Local to Azure Mapping

- Event Hubs emulator -> Azure Event Hubs namespace
- Azurite Blob -> Azure Storage Blob container
- local SQL Server container -> Azure SQL Database
- Docker compose -> AKS + Helm
- manual local run -> Azure DevOps CI/CD

## Local Architecture

```mermaid
flowchart LR
    Client["HTTP client"] --> Api["Nlb.Workshop.Api"]
    Api --> EventHub["Event Hubs emulator"]
    Worker["Nlb.Workshop.Consumer.Worker"] --> EventHub
    Worker --> SqlServer["SQL Server read model"]
    Replay["Nlb.Workshop.Tools.Replay"] --> EventHub
    Replay --> SqlServer
    Api --> SqlServer
```

## Azure Target Architecture

```mermaid
flowchart LR
    DevOps["Azure DevOps"] --> ACR["Azure Container Registry"]
    DevOps --> AKS["Azure Kubernetes Service"]
    DevOps --> Bicep["Bicep deployments"]
    Bicep --> RG["Resource Group"]
    RG --> EH["Azure Event Hubs"]
    RG --> SB["Azure Service Bus"]
    RG --> SA["Storage Account"]
    RG --> KV["Key Vault"]
    RG --> SQL["Azure SQL"]
    RG --> ACR
    RG --> AKS
    AKS --> Api["API pod"]
    AKS --> Worker["Consumer pod"]
    Api --> EH
    Worker --> EH
    Api --> SQL
    Worker --> SQL
    Worker --> SA
    Api --> KV
    Worker --> KV
```

## CI Flow

```mermaid
flowchart LR
    Commit["Commit to main"] --> CI["CI pipeline"]
    CI --> Restore["Restore + Build"]
    CI --> Validate["Bicep build + Helm lint"]
    CI --> Images["Docker build and push"]
    CI --> Artifact["Deployment artifact"]
```

## CD Flow

```mermaid
flowchart LR
    Artifact["CI artifact"] --> CD["CD pipeline"]
    CD --> Sub["Subscription deployment"]
    CD --> RG["Resource-group what-if and deploy"]
    CD --> Helm["Helm upgrade --install"]
    Helm --> Smoke["Smoke test /health/ready"]
```

## Runtime Secret Model

- infra creates runtime connection strings and stores them in Key Vault
- AKS Secret Store CSI driver syncs them into a Kubernetes secret
- Helm injects them as environment variables for API and worker
- the app itself reads resolved values from standard configuration

## Why Azure SQL

The workshop now uses SQL Server locally and Azure SQL in deployment. That keeps the same relational model and migration path across environments instead of teaching one database engine locally and another in production.
