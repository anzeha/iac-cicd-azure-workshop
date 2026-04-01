# Workshop Outline

## Audience

- .NET engineers who want to move an application from local infrastructure to Azure
- platform-minded engineers who need a practical Bicep and CI/CD example

## Session 1: Infrastructure as Code

### Goal

Move from local-only infrastructure to an Azure target architecture defined entirely in Bicep.

### Agenda

1. Review the current local workshop architecture
2. Identify local-only elements
3. Introduce Bicep structure: entrypoints, modules, params, outputs
4. Provision the Azure baseline:
   - resource group
   - Log Analytics
   - ACR
   - managed identity
   - Key Vault
   - Storage account with Blob + Queue
   - Event Hubs namespace and hub
   - Service Bus namespace, queue, and topic
   - Azure SQL
   - AKS
5. Review deployment outputs and generated runtime dependencies
6. Inspect resources in Azure Portal and Azure CLI

### Hands-on outcomes

- participants understand the `infra/` layout
- participants can run `what-if` and deploy both entrypoints
- participants know which outputs feed Helm and CD

## Session 2: AKS and Azure DevOps

### Goal

Package the application for AKS and automate build, deployment, and smoke checks in Azure DevOps.

### Agenda

1. Review cloud-readiness changes in the app
2. Explain the shared SQL Server -> Azure SQL migration path
3. Walk through Helm chart structure
4. Build the CI pipeline:
   - restore
   - build
   - validate Bicep
   - lint Helm
   - build and push images
   - publish deployment artifact
5. Build the CD pipeline:
   - deploy resource group
   - run `what-if`
   - deploy infra
   - deploy Helm release to AKS
   - run smoke test
6. Show monitoring and operational verification

### Hands-on outcomes

- a commit to `main` triggers CI
- CI publishes immutable images and deployment metadata
- CD deploys the single production environment from the CI artifact

## Suggested Timing

- Session 1: 3 hours
- Session 2: 3 hours

## Exercises

1. Change the workload prefix and deploy a clean environment
2. Add one new resource tag and confirm it appears on all resources
3. Change the API ingress host
4. Trigger a new CI run and redeploy the same artifact
5. Break a config setting intentionally and diagnose the readiness failure
