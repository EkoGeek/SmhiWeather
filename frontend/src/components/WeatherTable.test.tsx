import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
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

const stations: WeatherStationReading[] = [
  {
    stationId: '1',
    stationName: 'Malmö',
    latitude: 55.6,
    longitude: 13.0,
    temperature: [{ value: 20, unit: 'celsius', measuredAt: '2026-08-16T12:00:00Z', quality: 'G' }],
    windGust: [],
  },
  {
    stationId: '2',
    stationName: 'Kiruna',
    latitude: 67.85,
    longitude: 20.22,
    temperature: [{ value: 10, unit: 'celsius', measuredAt: '2026-08-16T12:00:00Z', quality: 'G' }],
    windGust: [],
  },
  {
    stationId: '3',
    stationName: 'Stockholm',
    latitude: 59.33,
    longitude: 18.06,
    temperature: [],
    windGust: [],
  },
]

function rowStationCellTexts(): string[] {
  return screen
    .getAllByRole('row')
    .slice(1) // drop the header row
    .map((row) => row.querySelector('td')?.textContent ?? '')
}

describe('WeatherTable', () => {
  it('shows an empty-state message when there are no readings', () => {
    render(<WeatherTable readings={[]} />)

    expect(screen.getByText(/no readings found/i)).toBeInTheDocument()
  })

  it('renders a row per station, with a dash for a missing parameter', () => {
    render(<WeatherTable readings={[hourReading]} />)

    expect(screen.getByText(/Stockholm-Observatoriekullen A/)).toBeInTheDocument()
    expect(screen.getByText('18.4°C')).toBeInTheDocument()
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
    expect(screen.getByText('9.7 m/s')).toBeInTheDocument()
  })

  it('shows the latest value plus an expandable series when there is more than one reading', () => {
    render(<WeatherTable readings={[dayReading]} />)

    expect(screen.getByText('18.4°C')).toBeInTheDocument()
    expect(screen.getByText('(3 readings)')).toBeInTheDocument()
    expect(screen.getByText(/26.5°C at/)).toBeInTheDocument()
  })

  it('expands a multi-reading series by default, without needing a click', () => {
    render(<WeatherTable readings={[dayReading]} />)

    const details = screen.getByText('(3 readings)').closest('details')
    expect(details).toHaveAttribute('open')
  })

  it('lists the expanded series newest-first', () => {
    render(<WeatherTable readings={[dayReading]} />)

    const listedValues = screen.getAllByRole('listitem').map((item) => item.textContent)
    expect(listedValues).toEqual([
      expect.stringContaining('18.4°C'),
      expect.stringContaining('25.1°C'),
      expect.stringContaining('26.5°C'),
    ])
  })

  it('sorts by station name ascending, then descending on a second click', async () => {
    const user = userEvent.setup()
    render(<WeatherTable readings={stations} />)

    await user.click(screen.getByRole('button', { name: /station/i }))
    expect(rowStationCellTexts()).toEqual([
      expect.stringContaining('Kiruna'),
      expect.stringContaining('Malmö'),
      expect.stringContaining('Stockholm'),
    ])

    await user.click(screen.getByRole('button', { name: /station/i }))
    expect(rowStationCellTexts()).toEqual([
      expect.stringContaining('Stockholm'),
      expect.stringContaining('Malmö'),
      expect.stringContaining('Kiruna'),
    ])
  })

  it('sorts temperature numerically, with missing values sinking to the bottom', async () => {
    const user = userEvent.setup()
    render(<WeatherTable readings={stations} />)

    await user.click(screen.getByRole('button', { name: /temperature/i }))
    expect(rowStationCellTexts()).toEqual([
      expect.stringContaining('Kiruna'), // 10°C
      expect.stringContaining('Malmö'), // 20°C
      expect.stringContaining('Stockholm'), // no reading - always last
    ])
  })

  it('sorts location south to north, then north to south on a second click', async () => {
    const user = userEvent.setup()
    render(<WeatherTable readings={stations} />)

    const locationHeader = screen.getByRole('button', { name: /location/i })

    await user.click(locationHeader)
    expect(rowStationCellTexts()).toEqual([
      expect.stringContaining('Malmö'), // 55.6N
      expect.stringContaining('Stockholm'), // 59.33N
      expect.stringContaining('Kiruna'), // 67.85N
    ])

    await user.click(locationHeader)
    expect(rowStationCellTexts()).toEqual([
      expect.stringContaining('Kiruna'),
      expect.stringContaining('Stockholm'),
      expect.stringContaining('Malmö'),
    ])
  })

  it('calls onSelectStation with the station id when its row is clicked', async () => {
    const user = userEvent.setup()
    const onSelectStation = vi.fn()
    render(<WeatherTable readings={[hourReading]} onSelectStation={onSelectStation} />)

    await user.click(screen.getByRole('row', { name: /Stockholm-Observatoriekullen A/i }))

    expect(onSelectStation).toHaveBeenCalledExactlyOnceWith('98230')
  })

  it('activates a row via keyboard with Enter or Space', async () => {
    const user = userEvent.setup()
    const onSelectStation = vi.fn()
    render(<WeatherTable readings={[hourReading]} onSelectStation={onSelectStation} />)

    screen.getByRole('row', { name: /Stockholm-Observatoriekullen A/i }).focus()
    await user.keyboard('{Enter}')

    expect(onSelectStation).toHaveBeenCalledExactlyOnceWith('98230')
  })

  it('does not select the row when expanding a multi-reading series', async () => {
    const user = userEvent.setup()
    const onSelectStation = vi.fn()
    render(<WeatherTable readings={[dayReading]} onSelectStation={onSelectStation} />)

    await user.click(screen.getByText('(3 readings)'))

    expect(onSelectStation).not.toHaveBeenCalled()
  })

  it('renders plain, non-interactive rows when onSelectStation is not provided', () => {
    render(<WeatherTable readings={[hourReading]} />)

    const dataRow = screen.getAllByRole('row')[1]
    expect(dataRow).not.toHaveAttribute('tabindex')
  })
})
