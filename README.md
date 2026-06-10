# Avaliacao Tecnica Sabemi

Implementacao de recebimento e processamento de webhooks de pagamento usando .NET 8, Clean Architecture, DDD, PostgreSQL, EF Core, Serilog, FluentValidation, Swagger e dashboard React.

## Estrutura

```text
src/backend/Sabemi.PaymentWebhook.Api            # HTTP, Swagger, middleware ApiKey
src/backend/Sabemi.PaymentWebhook.Application    # Casos de uso, DTOs, validadores, portas
src/backend/Sabemi.PaymentWebhook.Domain         # Entidades e regras de dominio
src/backend/Sabemi.PaymentWebhook.Infrastructure # EF Core, PostgreSQL, repositorios, background worker
src/frontend                                     # React + Vite + TypeScript
tests/Sabemi.PaymentWebhook.Tests               # Testes xUnit
docs                                            # Diagramas Mermaid
```

## Execucao com Docker

```bash
docker compose up --build
```

Servicos:

- Backend: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`
- Frontend: `http://localhost:3000`
- PostgreSQL: `localhost:5432`

A API aplica as migrations automaticamente ao iniciar.

## Webhook

Endpoint:

```http
POST /api/webhooks/pagamento
X-API-KEY: sabemi-dev-api-key
Content-Type: application/json
```

Payload:

```json
{
  "id_transacao": "TX123456",
  "id_contrato": "CTR001",
  "valor": 1500.50,
  "data_pagamento": "2026-06-10T10:00:00",
  "status": "SUCESSO"
}
```

Respostas:

- `202 Accepted`: evento aceito e enfileirado.
- `200 OK`: `id_transacao` duplicado, sem reprocessamento.
- `400 Bad Request`: payload invalido persistido com erro.
- `401 Unauthorized`: API key ausente ou invalida.

## Dashboard

O dashboard consome:

```http
GET /api/webhook-events?status=SUCESSO&idContrato=CTR001
```

Recursos:

- filtro por status;
- filtro por contrato;
- destaque visual para erro;
- atualizacao automatica a cada 5 segundos;
- exibicao de falhas de carregamento.

## Banco de Dados

Tabelas:

- `webhook_events`
- `contract_status`

Indices unicos:

- `ux_webhook_events_transaction_id`
- `ux_contract_status_contract_id`

## Testes

```bash
dotnet test Sabemi.slnx
```

Observacao: o frontend foi criado completo, mas este ambiente local nao possui Node/npm instalados. Para validar o frontend fora do Docker:

```bash
cd src/frontend
npm install
npm run build
```

## Diagramas

Os diagramas Mermaid estao em [docs/architecture.md](docs/architecture.md).
