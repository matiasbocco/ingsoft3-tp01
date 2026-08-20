import { render, screen } from '@testing-library/react'
import { describe, it, expect } from 'vitest'
import StockForm from '../components/StockForm'

describe('StockForm', () => {
  it('renders all required fields', () => {
    render(<StockForm onSuccess={() => {}} />)

    expect(screen.getByLabelText(/nombre/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/cantidad/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/ubicaci/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/categor/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /agregar/i })).toBeInTheDocument()
  })

  it('inputs start empty', () => {
    render(<StockForm onSuccess={() => {}} />)

    expect(screen.getByLabelText(/nombre/i)).toHaveValue('')
    expect(screen.getByLabelText(/cantidad/i)).toHaveValue(null)
    expect(screen.getByLabelText(/ubicaci/i)).toHaveValue('')
    expect(screen.getByLabelText(/categor/i)).toHaveValue('')
  })
})
