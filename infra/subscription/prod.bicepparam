using './main.bicep'

param location = 'westeurope'
param environmentName = 'prod'
param workloadName = 'nlb-workshop'
param tags = {
  owner: 'workshop'
  costCenter: 'engineering'
}
