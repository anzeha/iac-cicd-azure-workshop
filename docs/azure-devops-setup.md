# Azure DevOps Setup

## Create the pipelines

Create two YAML pipelines in Azure DevOps:

1. `nlb-workshop-azure-ci` -> points to `azure-pipelines-ci.yml`
2. `nlb-workshop-azure-cd` -> points to `azure-pipelines-cd.yml`

The CD pipeline resource in `azure-pipelines-cd.yml` expects the CI pipeline name to be `nlb-workshop-azure-ci`.

## Service connection

Create one Azure Resource Manager service connection and name it:

- `workshop-azure`

If you choose a different name, update the variable group value `azureRmServiceConnection`.

## Variable groups

Create these Azure DevOps variable groups.

### `workshop-common`

- `azureRmServiceConnection=workshop-azure`
- `acrName=<existing or prod ACR name>`
- `acrLoginServer=<acr-name>.azurecr.io`
- `location=westeurope`
- `workloadName=nlb-workshop`

### `workshop-prod`

- `sqlAdminPassword=<secret>`
- `apiIngressHost=workshop-api.example.com`

Mark `sqlAdminPassword` as secret.

## Environments

Create one environment:

- `workshop-prod`

Recommended:

- manual approval on `workshop-prod`

## Agent capabilities

Use Microsoft-hosted Ubuntu agents or self-hosted agents with:

- `dotnet`
- `az`
- `docker`
- `helm`
- `kubectl`

## AKS prerequisites outside the repo

The chart assumes:

- AKS has OIDC issuer enabled
- AKS has workload identity enabled
- AKS has the Azure Key Vault Secret Store CSI provider enabled
- an ingress controller exists in the cluster

If you use ingress-nginx, install it once per cluster before the first workshop deployment.

The CD pipeline smoke test now resolves the ingress address directly from the cluster and sends the configured
`apiIngressHost` as the `Host` header, so a public DNS record is optional for the first end-to-end deployment.
