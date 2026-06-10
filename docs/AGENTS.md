# AGENTS.md

## Arquitetura

Este projeto segue Clean Architecture:

- `Domain`: entidades e regras de negocio sem dependencia externa.
- `Application`: casos de uso, DTOs, validadores e interfaces de porta.
- `Infrastructure`: EF Core, PostgreSQL, repositorios, fila e background worker.
- `Api`: controllers, middleware, Swagger, Serilog e composicao da aplicacao.
- `frontend`: dashboard administrativo em React + Vite + TypeScript.

## Regras de Implementacao

- O Markdown `Sabemi_Spec_Driven_Development.md` e a fonte oficial dos requisitos.
- O endpoint oficial implementado e `POST /api/webhooks/pagamento`.
- Webhooks invalidos devem ser persistidos com `error_message`.
- Webhooks sem `X-API-KEY` valida nao devem ser persistidos.
- `id_transacao` e a chave de idempotencia e possui indice unico no banco.
- O endpoint responde rapidamente; o processamento pesado ocorre no `BackgroundService`.

## Padroes

- Casos de uso ficam em `Application/UseCases`.
- Dependencias externas sao acessadas por interfaces em `Application/Abstractions`.
- Mapeamentos EF ficam em `Infrastructure/Persistence/Configurations`.
- Logs devem ser estruturados, com propriedades como `TransactionId`, `ContractId` e `WebhookEventId`.
- Novos testes devem ficar em `tests/Sabemi.PaymentWebhook.Tests`.
