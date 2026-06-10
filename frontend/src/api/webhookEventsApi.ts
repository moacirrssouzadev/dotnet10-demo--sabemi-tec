import type { PaymentStatus, WebhookEvent } from '../types/webhookEvent';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:8080';
const API_KEY = import.meta.env.VITE_API_KEY ?? 'sabemi-dev-api-key';

export type WebhookEventFilters = {
  status?: PaymentStatus | '';
  contractId?: string;
};

export type PaymentWebhookPayload = {
  id_transacao: string;
  id_contrato: string;
  valor: number;
  data_pagamento: string;
  status: 'SUCESSO' | 'ERRO';
};

export async function listWebhookEvents(filters: WebhookEventFilters, signal?: AbortSignal): Promise<WebhookEvent[]> {
  const params = new URLSearchParams();

  if (filters.status) {
    params.set('status', filters.status);
  }

  if (filters.contractId?.trim()) {
    params.set('idContrato', filters.contractId.trim());
  }

  const response = await fetch(`${API_BASE_URL}/api/webhook-events?${params.toString()}`, { signal });

  if (!response.ok) {
    throw new Error(`Falha ao carregar pagamentos (${response.status})`);
  }

  return response.json();
}

export async function sendPaymentWebhook(payload: PaymentWebhookPayload): Promise<Response> {
  return fetch(`${API_BASE_URL}/api/webhooks/pagamento`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'X-API-KEY': API_KEY
    },
    body: JSON.stringify(payload)
  });
}


