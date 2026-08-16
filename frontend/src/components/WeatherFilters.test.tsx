import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { WeatherFilters } from './WeatherFilters'

describe('WeatherFilters', () => {
  it('does not show a clear button when the station ID is empty', () => {
    render(<WeatherFilters stationId="" period="hour" onChange={vi.fn()} />)

    expect(screen.queryByRole('button', { name: /clear station id/i })).not.toBeInTheDocument()
  })

  it('shows a clear button once a station ID is entered, and clears it on click', async () => {
    const user = userEvent.setup()
    render(<WeatherFilters stationId="" period="hour" onChange={vi.fn()} />)

    const input = screen.getByLabelText(/station id/i)
    await user.type(input, '98230')
    expect(input).toHaveValue('98230')

    await user.click(screen.getByRole('button', { name: /clear station id/i }))

    expect(input).toHaveValue('')
    expect(screen.queryByRole('button', { name: /clear station id/i })).not.toBeInTheDocument()
  })

  it('immediately switches to all stations when the clear button is clicked, without needing Apply', async () => {
    const user = userEvent.setup()
    const onChange = vi.fn()
    render(<WeatherFilters stationId="98230" period="hour" onChange={onChange} />)

    await user.click(screen.getByRole('button', { name: /clear station id/i }))

    expect(onChange).toHaveBeenCalledExactlyOnceWith({ stationId: '', period: 'hour' })
  })

  it('forces the period back to "hour" when clearing, since all-stations has no "day" feed', async () => {
    const user = userEvent.setup()
    const onChange = vi.fn()
    render(<WeatherFilters stationId="98230" period="day" onChange={onChange} />)

    await user.click(screen.getByRole('button', { name: /clear station id/i }))

    expect(onChange).toHaveBeenCalledExactlyOnceWith({ stationId: '', period: 'hour' })
  })
})
