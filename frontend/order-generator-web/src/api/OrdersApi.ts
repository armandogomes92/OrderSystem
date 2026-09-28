import type { FieldErrors, OrderRequest, OrderResponse } from '../orders/types'

const API_URL = import.meta.env.VITE_API_URL

export type SubmitResult =
  | { kind: 'success'; order: OrderResponse }
  | { kind: 'validation'; errors: FieldErrors }
  | { kind: 'error'; message: string }

interface ProblemDetails {
  detail?: string
  errors?: Record<string, string[]>
}

export async function submitOrder(request: OrderRequest): Promise<SubmitResult> {
  let response: Response

  try {
    response = await fetch(`${API_URL}/api/orders`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
    })
  } catch {
    return { kind: 'error', message: 'Não foi possível conectar ao OrderGenerator.' }
  }

  if (response.ok)
    return { kind: 'success', order: (await response.json()) as OrderResponse }

  const problem = (await response.json().catch(() => ({}))) as ProblemDetails

  if (response.status === 400 && problem.errors) {
    const errors: FieldErrors = {}
    for (const [field, messages] of Object.entries(problem.errors))
      errors[field as keyof FieldErrors] = messages[0]
    return { kind: 'validation', errors }
  }

  return { kind: 'error', message: problem.detail ?? `Erro inesperado (HTTP ${response.status}).` }
}