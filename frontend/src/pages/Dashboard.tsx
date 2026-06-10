import { RefreshCw, Search, XCircle } from 'lucide-react';
import { useCallback, useEffect, useMemo, useState } from 'react';
import { listWebhookEvents } from '../api/webhookEventsApi';
import { StatusBadge } from '../components/StatusBadge';
import type { PaymentStatus, WebhookEvent } from '../types/webhookEvent';

const currencyFormatter = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL'
});

const dateFormatter = new Intl.DateTimeFormat('pt-BR', {
  dateStyle: 'short',
  timeStyle: 'short'
});

export function Dashboard() {
  const [events, setEvents] = useState<WebhookEvent[]>([]);
  const [status, setStatus] = useState<PaymentStatus | ''>('');
  const [contractId, setContractId] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [lastUpdatedAt, setLastUpdatedAt] = useState<Date | null>(null);

  const loadEvents = useCallback(async (signal?: AbortSignal) => {
    setLoading(true);
    setError(null);

    try {
      const data = await listWebhookEvents({ status, contractId }, signal);
      setEvents(data);
      setLastUpdatedAt(new Date());
    } catch (loadError) {
      if (loadError instanceof DOMException && loadError.name === 'AbortError') {
        return;
      }

      setError(loadError instanceof Error ? loadError.message : 'Falha inesperada ao carregar pagamentos');
    } finally {
      setLoading(false);
    }
  }, [contractId, status]);

  useEffect(() => {
    const controller = new AbortController();
    loadEvents(controller.signal);

    const interval = window.setInterval(() => {
      loadEvents();
    }, 5000);

    return () => {
      controller.abort();
      window.clearInterval(interval);
    };
  }, [loadEvents]);

  const totals = useMemo(() => {
    return events.reduce(
      (accumulator, event) => {
        accumulator.total += 1;
        accumulator.errors += event.status === 'ERRO' || Boolean(event.errorMessage) ? 1 : 0;
        accumulator.processed += event.processed ? 1 : 0;
        return accumulator;
      },
      { total: 0, errors: 0, processed: 0 }
    );
  }, [events]);

  return (
    <main className="dashboard">
      <section className="dashboard__header">
        <div>
          <p className="eyebrow">Sabemi</p>
          <h1>Dashboard de pagamentos</h1>
        </div>
        <button className="icon-button" type="button" onClick={() => loadEvents()} title="Atualizar pagamentos">
          <RefreshCw size={18} aria-hidden="true" className={loading ? 'spin' : ''} />
          <span>Atualizar</span>
        </button>
      </section>

      <section className="metrics" aria-label="Resumo">
        <article>
          <span>Total</span>
          <strong>{totals.total}</strong>
        </article>
        <article>
          <span>Processados</span>
          <strong>{totals.processed}</strong>
        </article>
        <article className={totals.errors > 0 ? 'metric--alert' : ''}>
          <span>Com erro</span>
          <strong>{totals.errors}</strong>
        </article>
      </section>

      <section className="filters" aria-label="Filtros">
        <label>
          Status
          <select value={status} onChange={(event) => setStatus(event.target.value as PaymentStatus | '')}>
            <option value="">Todos</option>
            <option value="SUCESSO">SUCESSO</option>
            <option value="ERRO">ERRO</option>
          </select>
        </label>

        <label>
          Contrato
          <span className="input-with-icon">
            <Search size={16} aria-hidden="true" />
            <input
              value={contractId}
              onChange={(event) => setContractId(event.target.value)}
              placeholder="CTR001"
            />
          </span>
        </label>

        <p className="refresh-info">
          {lastUpdatedAt ? `Atualizado ${dateFormatter.format(lastUpdatedAt)}` : 'Aguardando dados'}
        </p>
      </section>

      {error && (
        <div className="error-banner" role="alert">
          <XCircle size={18} aria-hidden="true" />
          {error}
        </div>
      )}

      <section className="table-shell">
        <table>
          <thead>
            <tr>
              <th>ID Transacao</th>
              <th>ID Contrato</th>
              <th>Valor</th>
              <th>Data do Pagamento</th>
              <th>Status</th>
              <th>Processado</th>
              <th>Erro</th>
            </tr>
          </thead>
          <tbody>
            {events.map((event) => (
              <tr key={event.id} className={event.status === 'ERRO' || event.errorMessage ? 'row--error' : undefined}>
                <td>{event.transactionId ?? '-'}</td>
                <td>{event.contractId ?? '-'}</td>
                <td>{event.amount === null ? '-' : currencyFormatter.format(event.amount)}</td>
                <td>{event.paymentDate ? dateFormatter.format(new Date(event.paymentDate)) : '-'}</td>
                <td><StatusBadge status={event.status} /></td>
                <td>{event.processed ? 'Sim' : 'Nao'}</td>
                <td>{event.errorMessage ?? '-'}</td>
              </tr>
            ))}
            {events.length === 0 && (
              <tr>
                <td colSpan={7} className="empty-state">Nenhum pagamento encontrado</td>
              </tr>
            )}
          </tbody>
        </table>
      </section>
    </main>
  );
}
