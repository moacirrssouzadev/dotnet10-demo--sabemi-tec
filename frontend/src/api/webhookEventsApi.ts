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

export async function seedPaymentWebhookEvents(count = 10) {
  const now = new Date();

  const results = await Promise.allSettled(
    Array.from({ length: count }, (_, index) => {
      const payload: PaymentWebhookPayload = {
        id_transacao: `TX-${Date.now()}-${index + 1}`,
        id_contrato: `CTR00${(index % 5) + 1}`,
        valor: Number((Math.random() * 900 + 100).toFixed(2)),
        data_pagamento: new Date(now.getTime() - index * 10 * 60 * 1000).toISOString(),
        status: index % 4 === 0 ? 'ERRO' : 'SUCESSO'
      };

      return sendPaymentWebhook(payload);
    })
  );

  const failed = results.filter(result => result.status === 'rejected' || (result.status === 'fulfilled' && !result.value.ok));
  const errors = failed.map((result, index) => {
    if (result.status === 'rejected') {
      return `request ${index + 1} error: ${result.reason}`;
    }

    return `request ${index + 1} failed (${result.value.status})`;
  });

  return {
    success: count - failed.length,
    failed: failed.length,
    errors
  };
}
