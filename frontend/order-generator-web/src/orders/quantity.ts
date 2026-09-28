export const MAX_QUANTITY_DIGITS = 5

/** Mantém apenas dígitos e ignora a digitação além do limite do campo (99.999). */
export function toQuantityDigits(input: string): string | null {
  const digits = input.replace(/\D/g, '').replace(/^0+/, '')
  return digits.length > MAX_QUANTITY_DIGITS ? null : digits
}

/** Formata a quantidade no padrão brasileiro: "1500" → "1.500". */
export function formatQuantity(digits: string): string {
  return digits === '' ? '' : Number(digits).toLocaleString('pt-BR')
}
