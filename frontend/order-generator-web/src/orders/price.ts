export const MAX_PRICE_DIGITS = 5

const priceFormatter = new Intl.NumberFormat('pt-BR', {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
})

/**
 * Converte o texto digitado no campo de preço em centavos (apenas dígitos).
 * Cada novo dígito entra pela direita, deslocando os anteriores para a esquerda.
 * Retorna null quando o valor excede o limite do campo, para que a digitação seja ignorada.
 */
export function toPriceCents(input: string): string | null {
  const digits = input.replace(/\D/g, '').replace(/^0+/, '')
  return digits.length > MAX_PRICE_DIGITS ? null : digits
}

/** Formata centavos no padrão brasileiro: "1050" → "10,50". */
export function formatPriceCents(cents: string): string {
  return cents === '' ? '' : priceFormatter.format(Number(cents) / 100)
}

/** Converte centavos no valor enviado à API: "1050" → 10.5. */
export function priceCentsToNumber(cents: string): number {
  return Number(cents) / 100
}
