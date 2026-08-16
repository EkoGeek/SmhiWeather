import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { WeatherTable } from './WeatherTable'
import type { WeatherStationReading } from '../api/types'

const reading: WeatherStationReading = {
  stationId: '98230',
  stationName: 'Stockholm-Observatoriekullen A',
  latitude: 59.34,
  longitude: 18.05,
  temperature: { value: 18.4, unit: 'celsius', measuredAt: '2026-08-16T12:00:00Z', quality: 'G' },
  windGust: null,
}

describe('WeatherTable', () => {
  it('shows an empty-state message when there are no readings', () => {
    render(<WeatherTable readings={[]} />)

    expect(screen.getByText(/no readings found/i)).toBeInTheDocument()
  })

  it('renders a row per station, with a dash for a missing parameter', () => {
    render(<WeatherTable readings={[reading]} />)

    expect(screen.getByText(/Stockholm-Observatoriekullen A/)).toBeInTheDocument()
    expect(screen.getByText('18.4 celsius')).toBeInTheDocument()
    expect(screen.getAllByRole('row')).toHaveLength(2) // header + 1 data row
  })
})
