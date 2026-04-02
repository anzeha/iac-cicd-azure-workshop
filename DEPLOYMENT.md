```bash
export LOCATION=westeurope
export ENV=prod
export WORKLOAD=nlb-workshop
export RESOURCE_GROUP="rg-euw-sbx-espl-ws-01"
export DEPLOYMENT_NAME="workshop-${ENV}"
export API_HOST="workshop-api.example.com"
export SQL_ADMIN_PASSWORD=$(LC_ALL=C tr -dc 'A-Za-z0-9@#%+=:_-' </dev/urandom | head -c 24)
echo "$SQL_ADMIN_PASSWORD"
```

```bash
az login
az account set --subscription "<subscription-id-or-name>"
az account show -o table
```

```bash
az deployment group create \
  --name "$DEPLOYMENT_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --template-file infra/resource-group/main.bicep \
  --parameters infra/resource-group/prod.bicepparam \
  --parameters environmentName="$ENV" workloadName="$WORKLOAD" location="$LOCATION" \
  --parameters sqlAdminPassword="$SQL_ADMIN_PASSWORD"
```

```bash
az deployment group show \
  --resource-group "$RESOURCE_GROUP" \
  --name "$DEPLOYMENT_NAME" \
  --query properties.outputs
```

```bash
export ACR_NAME=$(az deployment group show --resource-group "$RESOURCE_GROUP" --name "$DEPLOYMENT_NAME" --query "properties.outputs.acrName.value" -o tsv)
export ACR_LOGIN_SERVER=$(az deployment group show --resource-group "$RESOURCE_GROUP" --name "$DEPLOYMENT_NAME" --query "properties.outputs.acrLoginServer.value" -o tsv)
export AKS_NAME=$(az deployment group show --resource-group "$RESOURCE_GROUP" --name "$DEPLOYMENT_NAME" --query "properties.outputs.aksClusterName.value" -o tsv)
export KEY_VAULT_NAME=$(az deployment group show --resource-group "$RESOURCE_GROUP" --name "$DEPLOYMENT_NAME" --query "properties.outputs.keyVaultName.value" -o tsv)
export WORKLOAD_IDENTITY_CLIENT_ID=$(az deployment group show --resource-group "$RESOURCE_GROUP" --name "$DEPLOYMENT_NAME" --query "properties.outputs.managedIdentityClientId.value" -o tsv)
export TENANT_ID=$(az account show --query tenantId -o tsv)

echo "$ACR_NAME"
echo "$ACR_LOGIN_SERVER"
echo "$AKS_NAME"
echo "$KEY_VAULT_NAME"
echo "$WORKLOAD_IDENTITY_CLIENT_ID"
echo "$TENANT_ID"
```

```bash
az aks update \
  --resource-group "$RESOURCE_GROUP" \
  --name "$AKS_NAME" \
  --attach-acr "$ACR_NAME"

az aks get-credentials \
  --resource-group "$RESOURCE_GROUP" \
  --name "$AKS_NAME" \
  --overwrite-existing
```

```bash
kubectl get nodes
kubectl get ns
```

```bash
helm list -n ingress-nginx
```

```bash
helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
helm repo update

helm upgrade --install ingress-nginx ingress-nginx/ingress-nginx \
  --namespace ingress-nginx \
  --create-namespace \
  --set controller.service.annotations."service\.beta\.kubernetes\.io/azure-load-balancer-health-probe-request-path"=/healthz
```

```bash
kubectl get pods -n ingress-nginx
kubectl get svc -n ingress-nginx
```

```bash
az acr login --name "$ACR_NAME"
```

```bash
docker buildx build \
  --platform linux/amd64 \
  -f Nlb.Workshop.Api/Dockerfile \
  -t "$ACR_LOGIN_SERVER/workshop-api:prod" \
  --push \
  .
```

```bash
docker buildx build \
  --platform linux/amd64 \
  -f Nlb.Workshop.Consumer.Worker/Dockerfile \
  -t "$ACR_LOGIN_SERVER/workshop-consumer:prod" \
  --push \
  .
```

```bash
helm upgrade --install workshop-app-prod helm/workshop-app \
  --namespace workshop-prod \
  --create-namespace \
  --values helm/workshop-app/values.yaml \
  --values helm/workshop-app/values-prod.yaml \
  --set global.environment=prod \
  --set global.workloadIdentity.clientId="$WORKLOAD_IDENTITY_CLIENT_ID" \
  --set global.workloadIdentity.tenantId="$TENANT_ID" \
  --set keyVault.name="$KEY_VAULT_NAME" \
  --set images.api.repository="workshop-api" \
  --set images.api.tag="prod" \
  --set images.consumer.repository="workshop-consumer" \
  --set images.consumer.tag="prod" \
  --set api.ingress.host="$API_HOST"
```
