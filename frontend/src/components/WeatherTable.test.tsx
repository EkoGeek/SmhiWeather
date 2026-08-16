import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { WeatherTable } from './WeatherTable'
import type { WeatherStationReading } from '../api/types'

const hourReading: WeatherStationReading = {
  stationId: '98230',
  stationName: 'Stockholm-Observatoriekullen A',
  latitude: 59.34,
  longitude: 18.05,
  temperature: [{ value: 18.4, unit: 'celsius', measuredAt: '2026-08-16T12:00:00Z', quality: 'G' }],
  windGust: [],
}

const dayReading: WeatherStationReading = {
  ...hourReading,
  temperature: [
    { value: 26.5, unit: 'celsius', measuredAt: '2026-08-16T09:00:00Z', quality: 'G' },
    { value: 25.1, unit: 'celsius', measuredAt: '2026-08-16T10:00:00Z', quality: 'G' },
    { value: 18.4, unit: 'celsius', measuredAt: '2026-08-16T12:00:00Z', quality: 'G' },
  ],
}

describe('WeatherTable', () => {
  it('shows an empty-state message when there are no readings', () => {
    render(<WeatherTable readings={[]} />)

    expect(screen.getByText(/no readings found/i)).toBeInTheDocument()
  })

  it('renders a row per station, with a dash for a missing parameter', () => {
    render(<WeatherTable readings={[hourReading]} />)

    expect(screen.getByText(/Stockholm-Observatoriekullen A/)).toBeInTheDocument()
    expect(screen.getByText('18.4 celsius')).toBeInTheDocument()
    expect(screen.getAllByRole('row')).toHaveLength(2) // header + 1 data row
  })

  it('shows the station location, and a dash when it is unknown', () => {
    const noLocationReading: WeatherStationReading = {
      ...hourReading,
      stationId: '2',
      latitude: null,
      longitude: null,
      windGust: [
        { value: 9.7, unit: 'meter per sekund', measuredAt: '2026-08-16T12:00:00Z', quality: 'G' },
      ],
    }
    render(<WeatherTable readings={[hourReading, noLocationReading]} />)

    expect(screen.getByText('59.3400, 18.0500')).toBeInTheDocument()
    expect(screen.getAllByText('—')).toHaveLength(2) // hourReading's missing wind gust + noLocationReading's location
  })

  it('shows the latest value plus an expandable series when there is more than one reading', () => {
    render(<WeatherTable readings={[dayReading]} />)

    expect(screen.getByText('18.4 celsius')).toBeInTheDocument()
    expect(screen.getByText('(3 readings)')).toBeInTheDocument()
    expect(screen.getByText(/26.5 celsius at/)).toBeInTheDocument()
  })

  it('lists the expanded series newest-first', () => {
    render(<WeatherTable readings={[dayReading]} />)

    const listedValues = screen.getAllByRole('listitem').map((item) => item.textContent)
    expect(listedValues).toEqual([
      expect.stringContaining('18.4 celsius'),
      expect.stringContaining('25.1 celsius'),
      expect.stringContaining('26.5 celsius'),
    ])
  })
})
