import { AlertTriangle, CheckCircle2 } from 'lucide-react';
import type { PaymentStatus } from '../types/webhookEvent';

type StatusBadgeProps = {
  status: PaymentStatus;
};

export function StatusBadge({ status }: StatusBadgeProps) {
  const isSuccess = status === 'SUCESSO';

  return (
    <span className={`status-badge ${isSuccess ? 'status-badge--success' : 'status-badge--error'}`}>
      {isSuccess ? <CheckCircle2 size={15} aria-hidden="true" /> : <AlertTriangle size={15} aria-hidden="true" />}
      {status}
    </span>
  );
}
