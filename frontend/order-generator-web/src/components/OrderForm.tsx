import { useState, type FormEvent, type ReactNode } from 'react'
import { submitOrder } from '../api/OrdersApi'
import { formatPriceCents, toPriceCents } from '../orders/price'
import { formatQuantity, toQuantityDigits } from '../orders/quantity'
import { SYMBOLS, type FieldErrors, type OrderFormValues, type OrderResponse, type OrderSide } from '../orders/types'
import { toOrderRequest, validateOrder } from '../orders/validations'
import { OrderResult } from './OrderResult'
import { OrderSummary } from './OrderSummary'
import { ResultModal } from './ResultModal'

const INITIAL_VALUES: OrderFormValues = { symbol: '', side: '', quantity: '', price: '' }

const SIDES: { value: OrderSide; label: string }[] = [
  { value: 'Buy', label: 'Compra' },
  { value: 'Sell', label: 'Venda' },
]

export function OrderForm() {
  const [values, setValues] = useState<OrderFormValues>(INITIAL_VALUES)
  const [errors, setErrors] = useState<FieldErrors>({})
  const [sentOrder, setSentOrder] = useState<OrderResponse | null>(null)
  const [submitError, setSubmitError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)

  function update(field: keyof OrderFormValues, value: string) {
    setValues((current) => ({ ...current, [field]: value }))
    setErrors((current) => ({ ...current, [field]: undefined }))
  }

  function updatePrice(input: string) {
    const cents = toPriceCents(input)
    if (cents !== null) update('price', cents)
  }

  function updateQuantity(input: string) {
    const digits = toQuantityDigits(input)
    if (digits !== null) update('quantity', digits)
  }

  const hasAnyValue = Object.values(values).some((value) => value !== '')

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setSubmitError(null)

    const validationErrors = validateOrder(values)
    setErrors(validationErrors)
    if (Object.keys(validationErrors).length > 0) return

    setSubmitting(true)
    try {
      const submitResult = await submitOrder(toOrderRequest(values))

      if (submitResult.kind === 'success') {
        setValues(INITIAL_VALUES)
        setErrors({})
        setSentOrder(submitResult.order)
      } else if (submitResult.kind === 'validation') {
        setErrors(submitResult.errors)
      } else {
        setSubmitError(submitResult.message)
      }
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="order-panel">
      <form className="order-form" onSubmit={handleSubmit} noValidate>
        <ChoiceGroup
          name="symbol"
          label="Símbolo"
          error={errors.symbol}
          options={SYMBOLS.map((symbol) => ({ value: symbol, label: symbol }))}
          value={values.symbol}
          onChange={(value) => update('symbol', value)}
        />

        <ChoiceGroup
          name="side"
          label="Lado"
          error={errors.side}
          options={SIDES}
          value={values.side}
          onChange={(value) => update('side', value)}
        />

        <Field id="quantity" label="Quantidade" error={errors.quantity}>
          <input
            id="quantity"
            type="text"
            inputMode="numeric"
            autoComplete="off"
            placeholder="0"
            value={formatQuantity(values.quantity)}
            aria-invalid={!!errors.quantity}
            onChange={(e) => updateQuantity(e.target.value)}
          />
        </Field>

        <Field id="price" label="Preço" error={errors.price}>
          <div className="price-input">
            <span aria-hidden="true">R$</span>
            <input
              id="price"
              type="text"
              inputMode="numeric"
              autoComplete="off"
              placeholder="0,00"
              value={formatPriceCents(values.price)}
              aria-invalid={!!errors.price}
              onChange={(e) => updatePrice(e.target.value)}
            />
          </div>
        </Field>

        <button type="submit" className="submit-button" disabled={submitting}>
          {submitting ? 'Enviando...' : 'Enviar ordem'}
        </button>
      </form>

      <aside className="order-aside">
        {hasAnyValue && <OrderSummary values={values} />}
        {submitError && <OrderResult result={{ kind: 'error', message: submitError }} />}
      </aside>

      <ResultModal order={sentOrder} onClose={() => setSentOrder(null)} />
    </div>
  )
}

interface FieldProps {
  id: string
  label: string
  error?: string
  children: ReactNode
}

function Field({ id, label, error, children }: FieldProps) {
  return (
    <div className="field">
      <label htmlFor={id} className="field__label">
        {label}
      </label>
      {children}
      {error && <span className="field__error">{error}</span>}
    </div>
  )
}

interface ChoiceGroupProps {
  name: string
  label: string
  error?: string
  options: { value: string; label: string }[]
  value: string
  onChange: (value: string) => void
}

function ChoiceGroup({ name, label, error, options, value, onChange }: ChoiceGroupProps) {
  return (
    <fieldset className="field" aria-invalid={!!error}>
      <legend className="field__label">
        {label}
      </legend>
      <div className="choices">
        {options.map((option) => (
          <label key={option.value} className="choice">
            <input
              type="radio"
              name={name}
              value={option.value}
              checked={value === option.value}
              onChange={() => onChange(option.value)}
            />
            <span>{option.label}</span>
          </label>
        ))}
      </div>
      {error && <span className="field__error">{error}</span>}
    </fieldset>
  )
}
