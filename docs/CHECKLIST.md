# Checklist de Requisitos

## Backend

- [x] .NET 8 Web API
- [x] Entity Framework Core
- [x] PostgreSQL
- [x] Swagger
- [x] FluentValidation
- [x] Serilog
- [x] Endpoint `POST /api/webhooks/pagamento`
- [x] Validacao por header `X-API-KEY`
- [x] Idempotencia por `id_transacao`
- [x] Indice unico para `transaction_id`
- [x] Tabela `webhook_events`
- [x] Tabela `contract_status`
- [x] Resposta imediata com `202 Accepted`
- [x] Processamento em `BackgroundService`
- [x] Simulacao de carga com `Task.Delay(2000)`
- [x] Logs estruturados de recebimento, duplicidade, processamento e erros

## Frontend

- [x] React + Vite + TypeScript
- [x] Dashboard de pagamentos
- [x] Filtro por status
- [x] Filtro por contrato
- [x] Exibicao de erros
- [x] Atualizacao automatica a cada 5 segundos

## Infraestrutura

- [x] Dockerfile Backend
- [x] Dockerfile Frontend
- [x] `docker-compose.yml`
- [x] PostgreSQL container

## Entregaveis

- [x] Estrutura completa do projeto
- [x] Codigo fonte
- [x] Migrations EF Core
- [x] README detalhado
- [x] Instrucoes de execucao
- [x] Diagramas Mermaid
- [x] AGENTS.md
- [x] Checklist
- [x] Testes unitarios xUnit
