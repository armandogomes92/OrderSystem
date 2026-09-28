import { priceCentsToNumber } from './price'
import { SYMBOLS, type FieldErrors, type OrderFormValues, type OrderRequest } from './types'

const DIGITS_PATTERN = /^\d+$/
const MAX_QUANTITY_EXCLUSIVE = 100_000
const MAX_PRICE_CENTS_EXCLUSIVE = 100_000

export function validateOrder(values: OrderFormValues): FieldErrors {
  const errors: FieldErrors = {}

  if (!SYMBOLS.includes(values.symbol as (typeof SYMBOLS)[number]))
    errors.symbol = 'Selecione PETR4, VALE3 ou VIIA4.'

  if (values.side !== 'Buy' && values.side !== 'Sell')
    errors.side = 'Selecione Compra ou Venda.'

  const quantity = Number(values.quantity)
  if (!DIGITS_PATTERN.test(values.quantity) || quantity <= 0 || quantity >= MAX_QUANTITY_EXCLUSIVE)
    errors.quantity = 'Informe um inteiro maior que zero e menor que 100.000.'

  const priceCents = Number(values.price)
  if (!DIGITS_PATTERN.test(values.price) || priceCents <= 0 || priceCents >= MAX_PRICE_CENTS_EXCLUSIVE)
    errors.price = 'Informe um preço maior que zero e menor que R$ 1.000,00.'

  return errors
}

export function toOrderRequest(values: OrderFormValues): OrderRequest {
  return {
    symbol: values.symbol,
    side: values.side as OrderRequest['side'],
    quantity: Number(values.quantity),
    price: priceCentsToNumber(values.price),
  }
}
