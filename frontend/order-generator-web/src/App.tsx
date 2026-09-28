import { OrderForm } from './components/OrderForm'

export default function App() {
  return (
    <>
      <header className="topbar">
        <div className="topbar__inner">
          <span className="brand">
            <span className="brand__mark" aria-hidden="true" />
            OrderGenerator
          </span>
        </div>
      </header>

      <main className="content">
        <div className="page-title">
          <h1>Nova ordem</h1>
        </div>
        <OrderForm />
      </main>
    </>
  )
}
