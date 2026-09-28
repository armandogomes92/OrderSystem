import { formatQuantity } from '../orders/quantity'
import { formatPriceCents } from '../orders/price'
import type { OrderFormValues } from '../orders/types'

const SIDE_LABEL = { Buy: 'Compra', Sell: 'Venda' } as const
const currency = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })

interface OrderSummaryProps {
  values: OrderFormValues
}

export function OrderSummary({ values }: OrderSummaryProps) {
  const hasVolume = values.quantity !== '' && values.price !== ''
  const volume = hasVolume ? (Number(values.quantity) * Number(values.price)) / 100 : null

  return (
    <section className="summary" aria-label="Resumo da ordem">
      <h2>Resumo da ordem</h2>
      <dl>
        <dt>Símbolo</dt>
        <dd>{values.symbol || '—'}</dd>
        <dt>Lado</dt>
        <dd>{values.side ? SIDE_LABEL[values.side] : '—'}</dd>
        <dt>Quantidade</dt>
        <dd>{formatQuantity(values.quantity) || '—'}</dd>
        <dt>Preço</dt>
        <dd>{values.price ? `R$ ${formatPriceCents(values.price)}` : '—'}</dd>
      </dl>
      <div className="summary__volume">
        <span>Volume financeiro</span>
        <strong>{volume === null ? '—' : currency.format(volume)}</strong>
      </div>
      <p className="summary__note">Limite de exposição por símbolo: R$ 100.000.000,00</p>
    </section>
  )
}
