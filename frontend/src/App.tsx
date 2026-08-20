import { useState, useCallback } from 'react'
import ItemList from './components/ItemList'
import StockForm from './components/StockForm'

function App() {
  const [categoria, setCategoria] = useState('')
  const [refreshKey, setRefreshKey] = useState(0)

  const handleStockAdded = useCallback(() => {
    setRefreshKey((k) => k + 1)
  }, [])

  return (
    <div style={{ maxWidth: 900, margin: '0 auto', padding: '2rem', fontFamily: 'sans-serif' }}>
      <h1>Inventario TP2</h1>

      <section>
        <h2>Agregar / Actualizar Stock</h2>
        <StockForm onSuccess={handleStockAdded} />
      </section>

      <section style={{ marginTop: '2rem' }}>
        <h2>Items en inventario</h2>
        <div style={{ marginBottom: '1rem' }}>
          <label htmlFor="filter-categoria">Filtrar por categoría: </label>
          <input
            id="filter-categoria"
            type="text"
            value={categoria}
            onChange={(e) => setCategoria(e.target.value)}
            placeholder="Ej: Electronica"
          />
        </div>
        <ItemList categoria={categoria} refreshKey={refreshKey} />
      </section>
    </div>
  )
}

export default App
