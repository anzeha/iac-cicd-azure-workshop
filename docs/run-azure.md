# Run Azure

This is the end-to-end participant runbook for the Azure version of the workshop.

## Prerequisites

- Azure subscription access
- Azure CLI
- Bicep CLI through Azure CLI
- Helm 3
- kubectl
- Docker
- Azure DevOps project with permission to create pipelines and variable groups

## 1. Restore and build locally first

```bash
cd /Users/pinokokol/Documents/GitHub/nlb-workshop-azure
dotnet restore nlb-workshop-azure.slnx
dotnet build nlb-workshop-azure.slnx
```

## 2. Sign in to Azure

```bash
az login
az account set --subscription "<your-subscription-name-or-id>"
```

## 3. Deploy the resource group

```bash
cd /Users/pinokokol/Documents/GitHub/nlb-workshop-azure
az deployment sub create \
  --location westeurope \
  --template-file infra/subscription/main.bicep \
  --parameters infra/subscription/prod.bicepparam
```

## 4. Deploy the environment resources

Pass the SQL admin password at deploy time.

```bash
cd /Users/pinokokol/Documents/GitHub/nlb-workshop-azure
az deployment group create \
  --name workshop-prod \
  --resource-group nlb-workshop-prod-rg \
  --template-file infra/resource-group/main.bicep \
  --parameters infra/resource-group/prod.bicepparam \
  --parameters sqlAdminPassword="<your-strong-password>"
```

## 5. Review deployment outputs

```bash
az deployment group show \
  --resource-group nlb-workshop-prod-rg \
  --name workshop-prod \
  --query properties.outputs
```

You need these values:

- ACR name and login server
- AKS cluster name
- Key Vault name
- managed identity client ID

## 6. Attach ACR to AKS and get cluster credentials

```bash
az aks update --resource-group nlb-workshop-prod-rg --name nlb-workshop-prod-aks --attach-acr <acr-name>
az aks get-credentials --resource-group nlb-workshop-prod-rg --name nlb-workshop-prod-aks --overwrite-existing
```

## 7. Install an ingress controller if your cluster does not already have one

Example with ingress-nginx:

```bash
helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
helm repo update
helm upgrade --install ingress-nginx ingress-nginx/ingress-nginx \
  --namespace ingress-nginx \
  --create-namespace
```

## 8. Build and push images

```bash
az acr login --name <acr-name>

docker build -f Nlb.Workshop.Api/Dockerfile -t <acr-login-server>/workshop-api:prod .
docker build -f Nlb.Workshop.Consumer.Worker/Dockerfile -t <acr-login-server>/workshop-consumer:prod .

docker push <acr-login-server>/workshop-api:prod
docker push <acr-login-server>/workshop-consumer:prod
```

## 9. Deploy the Helm release

```bash
helm upgrade --install workshop-app-prod helm/workshop-app \
  --namespace workshop-prod \
  --create-namespace \
  --values helm/workshop-app/values.yaml \
  --values helm/workshop-app/values-prod.yaml \
  --set global.environment=prod \
  --set global.workloadIdentity.clientId="<managed-identity-client-id>" \
  --set global.workloadIdentity.tenantId="$(az account show --query tenantId -o tsv)" \
  --set keyVault.name="<key-vault-name>" \
  --set images.api.repository="<acr-login-server>/workshop-api" \
  --set images.api.tag="prod" \
  --set images.consumer.repository="<acr-login-server>/workshop-consumer" \
  --set images.consumer.tag="prod" \
  --set api.ingress.host="workshop-api.example.com"
```

## 10. Verify workloads

```bash
kubectl get pods -n workshop-prod
kubectl get svc -n workshop-prod
kubectl get ingress -n workshop-prod
```

## 11. Smoke test the API

```bash
INGRESS_IP=$(kubectl get ingress workshop-app-prod-workshop-app-api \
  -n workshop-prod \
  -o jsonpath='{.status.loadBalancer.ingress[0].ip}')

curl -i \
  -H "Host: workshop-api.example.com" \
  "http://$INGRESS_IP/health/ready"
```

## 12. Publish an order through the Azure deployment

```bash
curl -s -X POST "http://$INGRESS_IP/orders" \
  -H "Host: workshop-api.example.com" \
  -H "Content-Type: application/json" \
  -d '{"orderId":null,"customerId":"customer-azure-001","amount":130.5,"currency":"EUR","correlationId":"azure-demo","useV2":true,"sourceSystem":"aks"}'
```

## 13. Set up Azure DevOps automation

Follow [docs/azure-devops-setup.md](azure-devops-setup.md), then create:

1. CI pipeline from `azure-pipelines-ci.yml`
2. CD pipeline from `azure-pipelines-cd.yml`

After that:

1. commit to `main`
2. let CI restore, build, validate, and publish images/artifacts
3. let CD deploy the single `prod` environment from that artifact

## Expected secret names in Key Vault

- `read-model-connection-string`
- `eventhubs-connection-string`
- `blob-checkpoint-connection-string`
- `servicebus-connection-string`
