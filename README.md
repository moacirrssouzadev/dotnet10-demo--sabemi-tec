# Avaliacao Tecnica Sabemi

Implementacao de recebimento e processamento de webhooks de pagamento usando .NET 10, Clean Architecture, DDD, PostgreSQL, EF Core, Serilog, FluentValidation e Swagger.

## Estrutura

```text
backend/src/Sabemi.PaymentWebhook.Api            # HTTP, Swagger, middleware de autenticacao (API Key + Signature)
backend/src/Sabemi.PaymentWebhook.Application    # Casos de uso, DTOs, validadores, portas
backend/src/Sabemi.PaymentWebhook.Domain         # Entidades e regras de dominio
backend/src/Sabemi.PaymentWebhook.Infrastructure # EF Core, PostgreSQL, repositorios, background worker
backend/tests/Sabemi.PaymentWebhook.Tests        # Testes xUnit
```

## Execucao Local

1. **Restaurar dependências**:
   ```bash
   cd backend
   dotnet restore
   ```

2. **Iniciar a API**:
   ```bash
   cd backend/src/Sabemi.PaymentWebhook.Api
   dotnet run
   ```

Servicos:

- Backend: `http://localhost:5095`
- Swagger: `http://localhost:5095/swagger`

## Webhook

Endpoint:

```http
POST /webhooks/pagamento
Content-Type: application/json
```

### Autenticacao (2 metodos):

#### 1. API Key
```http
X-API-KEY: sabemi-dev-api-key
```

#### 2. Signature (HMAC-SHA256)
Calcula o HMAC-SHA256 do payload bruto usando o secret `sabemi-dev-signature-secret-123456`, converte para hexadecimal em lowercase:
```http
X-Signature: 0b1e0099db2325c261e9d9689545bd0c0d915e6ed569d28f97cdd8bdb8ee737e
```

Payload:

```json
{
  "id_transacao": "TRX-20260610-0001",
  "id_contrato": "CTR-123456",
  "valor": 1500.75,
  "data_pagamento": "2026-06-10T21:43:02.178Z",
  "status": "PAGO"
}
```

Respostas:

- `202 Accepted`: evento aceito.
- `200 OK`: `id_transacao` duplicado.
- `400 Bad Request`: payload invalido.
- `401 Unauthorized`: autenticacao falhou.

## Banco de Dados

Tabelas:

- `webhook_events`
- `contract_status`

## Testes

```bash
cd backend
dotnet test Sabemi.slnx
```
