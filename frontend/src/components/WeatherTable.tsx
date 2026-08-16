import { useMemo, useState } from 'react'
import type { ParameterReading, WeatherStationReading } from '../api/types'

const UNIT_SYMBOLS: Record<string, string> = {
  celsius: '°C',
  'meter per sekund': 'm/s',
}

function formatValue(reading: ParameterReading): string {
  const unit = UNIT_SYMBOLS[reading.unit] ?? reading.unit
  const separator = unit === '°C' ? '' : ' '
  return `${reading.value}${separator}${unit}`
}

function formatLocation(reading: WeatherStationReading): string {
  if (reading.latitude === null || reading.longitude === null) {
    return '—'
  }
  return `${reading.latitude.toFixed(4)}, ${reading.longitude.toFixed(4)}`
}

function formatTime(reading: ParameterReading): string {
  const date = new Date(reading.measuredAt)
  const pad = (n: number) => String(n).padStart(2, '0')
  const datePart = `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`
  const timePart = `${pad(date.getHours())}:${pad(date.getMinutes())}`
  return `${datePart} ${timePart}`
}

function latestOf(readings: ParameterReading[]): ParameterReading | undefined {
  return readings.at(-1)
}

/** Shows the latest reading; if there's a series (period=day), the rest are expanded by default. */
function ParameterCell({ readings }: { readings: ParameterReading[] }) {
  if (readings.length === 0) {
    return <span className="text-gray-500">—</span>
  }

  const latest = readings[readings.length - 1]

  if (readings.length === 1) {
    return <span>{formatValue(latest)}</span>
  }

  // SMHI returns measurements oldest-first; show newest-first here so the most recent reading is on top.
  const newestFirst = [...readings].reverse()

  return (
    <details open>
      {/* Stop propagation so collapsing/expanding the series doesn't also trigger the row's onClick. */}
      <summary className="cursor-pointer" onClick={(event) => event.stopPropagation()}>
        {formatValue(latest)}{' '}
        <span className="text-sm text-gray-500">({readings.length} readings)</span>
      </summary>
      <ul className="mt-1 text-sm text-gray-600">
        {newestFirst.map((reading) => (
          <li key={reading.measuredAt}>
            {formatValue(reading)} at {formatTime(reading)}
          </li>
        ))}
      </ul>
    </details>
  )
}

type SortColumn = 'station' | 'temperature' | 'windGust' | 'location' | 'measuredAt'
type SortDirection = 'asc' | 'desc'
interface SortState {
  column: SortColumn
  direction: SortDirection
}

/** Numbers sort first by direction; readings missing that value always sink to the bottom. */
function compareNullableNumber(
  a: number | null,
  b: number | null,
  direction: SortDirection,
): number {
  if (a === null && b === null) return 0
  if (a === null) return 1
  if (b === null) return -1
  return direction === 'asc' ? a - b : b - a
}

function latestValue(readings: ParameterReading[]): number | null {
  return latestOf(readings)?.value ?? null
}

function latestTimestamp(reading: WeatherStationReading): number | null {
  const latest = latestOf(reading.temperature) ?? latestOf(reading.windGust)
  return latest ? new Date(latest.measuredAt).getTime() : null
}

function getComparator(
  column: SortColumn,
  direction: SortDirection,
): (a: WeatherStationReading, b: WeatherStationReading) => number {
  switch (column) {
    case 'station':
      return (a, b) =>
        direction === 'asc'
          ? a.stationName.localeCompare(b.stationName)
          : b.stationName.localeCompare(a.stationName)
    case 'temperature':
      return (a, b) =>
        compareNullableNumber(latestValue(a.temperature), latestValue(b.temperature), direction)
    case 'windGust':
      return (a, b) =>
        compareNullableNumber(latestValue(a.windGust), latestValue(b.windGust), direction)
    case 'location':
      // Ascending latitude = south to north; descending = north to south.
      return (a, b) => compareNullableNumber(a.latitude, b.latitude, direction)
    case 'measuredAt':
      return (a, b) => compareNullableNumber(latestTimestamp(a), latestTimestamp(b), direction)
  }
}

interface SortableHeaderProps {
  column: SortColumn
  label: string
  sort: SortState | null
  onSort: (column: SortColumn) => void
  /** Overrides the generic ▲/▼ indicator with direction-specific text, e.g. for Location. */
  directionLabels?: { asc: string; desc: string }
}

function SortableHeader({ column, label, sort, onSort, directionLabels }: SortableHeaderProps) {
  const isActive = sort?.column === column
  const direction = isActive ? sort.direction : null

  const indicator = directionLabels
    ? direction === 'asc'
      ? directionLabels.asc
      : direction === 'desc'
        ? directionLabels.desc
        : null
    : direction === 'asc'
      ? '▲'
      : direction === 'desc'
        ? '▼'
        : null

  return (
    <th
      scope="col"
      className="py-2 pr-4"
      aria-sort={direction === 'asc' ? 'ascending' : direction === 'desc' ? 'descending' : 'none'}
    >
      <button
        type="button"
        onClick={() => onSort(column)}
        className="flex items-center gap-1 font-medium hover:text-gray-900 focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500"
      >
        {label}
        <span aria-hidden="true" className="text-gray-400">
          {indicator ?? '↕'}
        </span>
      </button>
    </th>
  )
}

interface WeatherTableProps {
  readings: WeatherStationReading[]
  /** Called with a station's ID when its row is clicked or activated via keyboard. */
  onSelectStation?: (stationId: string) => void
}

export function WeatherTable({ readings, onSelectStation }: WeatherTableProps) {
  const [sort, setSort] = useState<SortState | null>(null)

  const sortedReadings = useMemo(() => {
    if (!sort) return readings
    return [...readings].sort(getComparator(sort.column, sort.direction))
  }, [readings, sort])

  function handleSort(column: SortColumn) {
    setSort((current) =>
      current?.column === column
        ? { column, direction: current.direction === 'asc' ? 'desc' : 'asc' }
        : { column, direction: 'asc' },
    )
  }

  if (readings.length === 0) {
    return <p className="text-gray-600">No readings found for the selected filters.</p>
  }

  return (
    <table className="w-full border-collapse text-left">
      <caption className="sr-only">Combined temperature and wind gust readings by station</caption>
      <thead>
        <tr className="border-b border-gray-300 text-sm text-gray-600">
          <SortableHeader column="station" label="Station" sort={sort} onSort={handleSort} />
          <SortableHeader
            column="temperature"
            label="Temperature"
            sort={sort}
            onSort={handleSort}
          />
          <SortableHeader column="windGust" label="Wind gust" sort={sort} onSort={handleSort} />
          <SortableHeader
            column="location"
            label="Location"
            sort={sort}
            onSort={handleSort}
            directionLabels={{ asc: 'S → N', desc: 'N → S' }}
          />
          <SortableHeader
            column="measuredAt"
            label="Latest measured at"
            sort={sort}
            onSort={handleSort}
          />
        </tr>
      </thead>
      <tbody>
        {sortedReadings.map((reading) => {
          const latest = latestOf(reading.temperature) ?? latestOf(reading.windGust)
          const selectStation = onSelectStation
            ? () => onSelectStation(reading.stationId)
            : undefined

          return (
            <tr
              key={reading.stationId}
              className={`border-b border-gray-100 align-top ${
                selectStation
                  ? 'cursor-pointer hover:bg-gray-50 focus:outline-none focus-visible:ring-2 focus-visible:ring-inset focus-visible:ring-blue-500'
                  : ''
              }`}
              tabIndex={selectStation ? 0 : undefined}
              onClick={selectStation}
              onKeyDown={
                selectStation
                  ? (event) => {
                      if (event.key === 'Enter' || event.key === ' ') {
                        event.preventDefault()
                        selectStation()
                      }
                    }
                  : undefined
              }
              aria-label={
                selectStation ? `Show latest day readings for ${reading.stationName}` : undefined
              }
            >
              <td className="py-2 pr-4">
                {reading.stationName}{' '}
                <span className="text-sm text-gray-500">({reading.stationId})</span>
              </td>
              <td className="py-2 pr-4">
                <ParameterCell readings={reading.temperature} />
              </td>
              <td className="py-2 pr-4">
                <ParameterCell readings={reading.windGust} />
              </td>
              <td className="py-2 pr-4 text-sm text-gray-500">{formatLocation(reading)}</td>
              <td className="py-2 pr-4 text-sm text-gray-500">
                {latest ? formatTime(latest) : ''}
              </td>
            </tr>
          )
        })}
      </tbody>
    </table>
  )
}
