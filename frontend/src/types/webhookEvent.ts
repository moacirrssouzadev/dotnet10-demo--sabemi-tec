export type PaymentStatus = 'SUCESSO' | 'ERRO';

export type WebhookEvent = {
  id: string;
  transactionId: string | null;
  contractId: string | null;
  amount: number | null;
  paymentDate: string | null;
  status: PaymentStatus;
  processed: boolean;
  errorMessage: string | null;
  createdAt: string;
  processedAt: string | null;
};
