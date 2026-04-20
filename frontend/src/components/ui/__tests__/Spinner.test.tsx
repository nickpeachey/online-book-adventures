import { render, screen } from '@testing-library/react'
import { Spinner } from '../Spinner'

describe('Spinner', () => {
  it('renders with accessible label', () => {
    render(<Spinner />)
    expect(screen.getByRole('status', { name: /loading/i })).toBeInTheDocument()
  })
})
