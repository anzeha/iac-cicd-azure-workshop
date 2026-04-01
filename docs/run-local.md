# Run Local

## Prerequisites

- .NET SDK 10
- Docker Desktop

## 1. Start local infrastructure

```bash
cd /Users/pinokokol/Documents/GitHub/nlb-workshop-azure
docker compose up -d eventhubs-emulator azurite schema-registry kafka-ui sqlserver
docker compose ps
```

Kafka UI:

- `http://localhost:8085`

## 2. Restore and build

```bash
cd /Users/pinokokol/Documents/GitHub/nlb-workshop-azure
dotnet restore nlb-workshop-azure.slnx
dotnet build nlb-workshop-azure.slnx
```

## 3. Start the worker

```bash
cd /Users/pinokokol/Documents/GitHub/nlb-workshop-azure
dotnet run --project Nlb.Workshop.Consumer.Worker
```

## 4. Start the API

```bash
cd /Users/pinokokol/Documents/GitHub/nlb-workshop-azure
dotnet run --project Nlb.Workshop.Api --urls http://localhost:5001
```

## 5. Verify health endpoints

```bash
curl -s http://localhost:5001/health
curl -s http://localhost:5001/health/live
curl -s http://localhost:5001/health/ready
```

## 6. Publish one order

```bash
curl -s -X POST http://localhost:5001/orders \
  -H "Content-Type: application/json" \
  -d '{"orderId":null,"customerId":"customer-001","amount":130.5,"currency":"EUR","correlationId":null,"useV2":false,"sourceSystem":null}'
```

## 7. Replay the read model

```bash
cd /Users/pinokokol/Documents/GitHub/nlb-workshop-azure
dotnet run --project Nlb.Workshop.Tools.Replay -- --reset-read-model
```

## Notes

- local mode uses SQL Server on `localhost:1433`
- the API, worker, and replay tool all use the same SQL Server migration path
