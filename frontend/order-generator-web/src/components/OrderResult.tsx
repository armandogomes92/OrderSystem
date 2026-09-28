import type { SubmitResult } from '../api/OrdersApi'

const SIDE_LABEL = { Buy: 'Compra', Sell: 'Venda' } as const
const currency = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })

interface OrderResultProps {
  result: SubmitResult
}

export function OrderResult({ result }: OrderResultProps) {
  if (result.kind === 'validation') return null

  if (result.kind === 'error')
    return (
      <section className="result result--error" role="alert">
        <span className="badge badge--error">Erro</span>
        <h2>Não foi possível enviar a ordem</h2>
        <p>{result.message}</p>
      </section>
    )

  const { order } = result
  const accepted = order.status === 'New'

  return (
    <section className={`result ${accepted ? 'result--accepted' : 'result--rejected'}`} role="status">
      <span className={`badge ${accepted ? 'badge--accepted' : 'badge--rejected'}`}>
        {accepted ? 'Aceita' : 'Rejeitada'}
      </span>
      <h2>{accepted ? 'Ordem registrada' : 'Ordem não registrada'}</h2>
      {order.rejectReason && <p>{order.rejectReason}</p>}
      <dl>
        <dt>Símbolo</dt>
        <dd>{order.symbol}</dd>
        <dt>Lado</dt>
        <dd>{SIDE_LABEL[order.side]}</dd>
        <dt>Quantidade</dt>
        <dd>{order.quantity.toLocaleString('pt-BR')}</dd>
        <dt>Preço</dt>
        <dd>{currency.format(order.price)}</dd>
        <dt>ExecType</dt>
        <dd>{order.status}</dd>
        <dt>ClOrdID</dt>
        <dd>{order.clOrdId}</dd>
        <dt>OrderID</dt>
        <dd>{order.orderId}</dd>
      </dl>
    </section>
  )
}