export const SYMBOLS = ['PETR4', 'VALE3', 'VIIA4'] as const

export type OrderSide = 'Buy' | 'Sell'
export type OrderStatus = 'New' | 'Rejected'

export interface OrderFormValues {
  symbol: string
  side: OrderSide | ''
  quantity: string
  /** Preço em centavos, somente dígitos (ex.: "1050" = R$ 10,50). */
  price: string
}

export interface OrderRequest {
  symbol: string
  side: OrderSide
  quantity: number
  price: number
}

export interface OrderResponse {
  clOrdId: string
  orderId: string
  status: OrderStatus
  symbol: string
  side: OrderSide
  quantity: number
  price: number
  rejectReason: string | null
}

export type FieldErrors = Partial<Record<keyof OrderRequest, string>>