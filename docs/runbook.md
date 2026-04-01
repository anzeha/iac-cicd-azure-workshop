# Workshop Runbook

Use this checklist during the workshop delivery.

## Before Session 1

1. Confirm Azure subscription access
2. Confirm Azure DevOps project access
3. Confirm `az`, `docker`, `helm`, `kubectl`, and `.NET 10` are installed
4. Confirm the repo builds locally
5. Confirm the SQL admin password chosen for the exercise

## During Session 1

1. Deploy subscription-scope Bicep
2. Deploy resource-group-scope Bicep
3. Show outputs and explain how they drive later deployment
4. Open Azure Portal and inspect:
   - Event Hubs
   - Service Bus
   - Storage
   - Key Vault
   - Azure SQL
   - ACR
   - AKS

## Before Session 2

1. Confirm ingress controller is installed in AKS
2. Confirm ACR push permissions work
3. Confirm AKS can pull from ACR
4. Confirm Key Vault CSI provider is enabled in the cluster

## During Session 2

1. Build and push API image
2. Build and push consumer image
3. Deploy Helm release
4. Verify pods become ready
5. Hit `/health/ready`
6. Publish one order
7. Check worker logs
8. Check the read model in Azure SQL
9. Create CI and CD pipelines in Azure DevOps
10. Trigger a pipeline run from `main`

## End of Workshop

1. Export the deployment outputs
2. Save the pipeline run links
3. If the environment is temporary, delete the resource group
