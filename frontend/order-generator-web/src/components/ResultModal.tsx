import { useEffect, useRef } from 'react'
import type { OrderResponse } from '../orders/types'
import { OrderResult } from './OrderResult'

interface ResultModalProps {
  order: OrderResponse | null
  onClose: () => void
}

export function ResultModal({ order, onClose }: ResultModalProps) {
  const dialogRef = useRef<HTMLDialogElement>(null)

  useEffect(() => {
    const dialog = dialogRef.current
    if (!dialog) return

    if (order && !dialog.open) dialog.showModal()
    if (!order && dialog.open) dialog.close()
  }, [order])

  return (
    <dialog ref={dialogRef} className="modal" aria-label="Resultado da ordem" onClose={onClose}>
      {order && (
        <>
          <OrderResult result={{ kind: 'success', order }} />
          <div className="modal__actions">
            <button type="button" className="submit-button" autoFocus onClick={() => dialogRef.current?.close()}>
              OK
            </button>
          </div>
        </>
      )}
    </dialog>
  )
}
