# Avaliação Técnica Sabemi - Spec Driven Development

## Feature: Recebimento e Processamento de Webhooks de Pagamento

Como um serviço de processamento de pagamentos da Sabemi,
quero receber notificações de pagamento de um banco parceiro,
garantir que o mesmo evento não seja processado duas vezes,
registrar o histórico de eventos e exibir o resultado em um painel administrativo.

## Cenários

### Cenário 1: Receber webhook válido e processar em background
Dado que o banco parceiro envia uma requisição POST para `/webhooks/pagamento` com um payload válido
E o cabeçalho de autenticação está presente e válido
Quando o endpoint recebe o evento
Então o serviço deve persistir o evento bruto em `webhook_events`
E responder rapidamente com HTTP 202 Accepted
E enfileirar o processamento do evento em background
E atualizar o `contract_status` quando o processamento for concluído com sucesso.

### Cenário 2: Bloquear requisição sem chave de API válida
Dado que a requisição não contém a chave de API esperada
Quando a chamada ao endpoint for recebida
Então o serviço deve responder com HTTP 401 Unauthorized
E não deve persistir o evento em `webhook_events`.

### Cenário 3: Ignorar webhook duplicado por id_transacao
Dado que um evento com `id_transacao` já foi recebido anteriormente
Quando o mesmo `id_transacao` for enviado novamente
Então o serviço não deve processar o evento duplicado novamente
E deve responder com HTTP 200 OK ou 202 Accepted conforme o contract design
E deve manter apenas uma entrada original de processamento.

### Cenário 4: Registrar falha de validação e sinalizar no dashboard
Dado que o payload do webhook falha na validação de campos obrigatórios ou formato
Quando o evento for recebido
Então o serviço deve persistir o evento bruto com status de erro
E armazenar a mensagem de erro em `error_message`
E exibir o evento com alerta visual no painel administrativo.

### Cenário 5: Dashboard lista eventos e aplica filtros
Dado que existem eventos recebidos no sistema
Quando o usuário abrir o painel de dashboard
Então o painel deve listar os pagamentos recebidos
E permitir filtrar por `status` (SUCESSO / ERRO)
E permitir filtrar por `id_contrato`
E ressaltar visualmente os registros com erro.

## Requisitos Técnicos

### Backend (.NET)
- Endpoint de Recebimento: `POST /webhooks/pagamento`
- Payload esperado:
  - `id_transacao` (string)
  - `id_contrato` (string)
  - `valor` (decimal)
  - `data_pagamento` (datetime)
  - `status` (string)
- Segurança: validação de `ApiKey` ou `Signature` via cabeçalho HTTP
- Idempotência: impedir processamento duplo do mesmo `id_transacao`
- Persistência: tabela de eventos brutos e tabela de status do contrato
- Resiliência: responder rápido e processar regras de negócio em background
- Simulação de carga: atraso controlado de ~2 segundos durante o processamento

### Frontend (React)
- Tela de Dashboard simples que lista eventos
- Filtros por status e ID do contrato
- Visualização clara de eventos com erro
- Atualização em tempo real ou refresh manual

## Fluxo de Processamento

1. Receber POST em `/webhooks/pagamento`
2. Validar cabeçalho de segurança
3. Validar payload do webhook
4. Verificar idempotência pelo `id_transacao`
5. Persistir o evento em `webhook_events`
6. Retornar HTTP 202 Accepted imediatamente
7. Processar o evento em background
8. Atualizar `contract_status` e marcar o evento como processado ou com erro

## API

### POST /webhooks/pagamento
Headers:
- `X-API-KEY: <api-key>`

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

Respostas esperadas:
- `202 Accepted` para webhook válido aceito para processamento
- `401 Unauthorized` para autenticação inválida
- `400 Bad Request` para payload inválido
- `200 OK` ou `202 Accepted` para webhook duplicado não reprocessado

## Modelo de Dados

### `webhook_events`
- `id` UUID PK
- `transaction_id` VARCHAR(100) UNIQUE
- `contract_id` VARCHAR(100)
- `amount` NUMERIC(18,2)
- `payment_date` TIMESTAMP
- `status` VARCHAR(50)
- `payload` JSONB
- `processed` BOOLEAN NOT NULL DEFAULT FALSE
- `error_message` TEXT NULL
- `created_at` TIMESTAMP NOT NULL DEFAULT now()
- `processed_at` TIMESTAMP NULL

### `contract_status`
- `id` UUID PK
- `contract_id` VARCHAR(100) UNIQUE
- `last_transaction_id` VARCHAR(100)
- `status` VARCHAR(50)
- `amount` NUMERIC(18,2)
- `last_payment_date` TIMESTAMP
- `updated_at` TIMESTAMP NOT NULL DEFAULT now()

## Arquitetura e Organização

- `Api`: controladores e endpoints
- `Application`: casos de uso, serviços de domínio e orquestração
- `Domain`: entidades, agregados e regras de negócio
- `Infrastructure`: persistência, background worker e integração com banco
- `Tests`: testes de unidade e integração com xUnit

## Dashboard Administrativo

Colunas da tabela:
- ID Transação
- ID Contrato
- Valor
- Data do Pagamento
- Status do Evento
- Processado
- Erro

Filtros:
- `status` (SUCESSO / ERRO)
- `id_contrato`

Comportamento:
- Eventos de erro devem aparecer com destaque visual
- Pagamentos processados com sucesso devem ser marcados claramente
- O painel pode atualizar automaticamente ou por refresh manual

## Critérios de Aceitação

- O endpoint aceita apenas eventos autenticados
- Eventos válidos são registrados e processados em background
- Eventos duplicados não são reprocessados
- Falhas de validação são armazenadas e visíveis no dashboard
- O dashboard permite filtrar e visualizar o histórico de pagamentos
- Clean Architecture e DDD + SOLID + CLEAN CODE
- Testes automatizados de unidade e integração
- Documentação Swagger para a API
- Uso de Docker para desenvolvimento e testes locais