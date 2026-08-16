import type { WeatherStationReading } from '../api/types'

function formatReading(reading: WeatherStationReading['temperature']): string {
  if (!reading) return '—'
  return `${reading.value} ${reading.unit}`
}

function formatMeasuredAt(reading: WeatherStationReading['temperature']): string {
  if (!reading) return ''
  return new Date(reading.measuredAt).toLocaleString()
}

interface WeatherTableProps {
  readings: WeatherStationReading[]
}

export function WeatherTable({ readings }: WeatherTableProps) {
  if (readings.length === 0) {
    return <p className="text-gray-600">No readings found for the selected filters.</p>
  }

  return (
    <table className="w-full border-collapse text-left">
      <caption className="sr-only">Combined temperature and wind gust readings by station</caption>
      <thead>
        <tr className="border-b border-gray-300 text-sm text-gray-600">
          <th scope="col" className="py-2 pr-4">
            Station
          </th>
          <th scope="col" className="py-2 pr-4">
            Temperature
          </th>
          <th scope="col" className="py-2 pr-4">
            Wind gust
          </th>
          <th scope="col" className="py-2 pr-4">
            Measured at
          </th>
        </tr>
      </thead>
      <tbody>
        {readings.map((reading) => (
          <tr key={reading.stationId} className="border-b border-gray-100">
            <td className="py-2 pr-4">
              {reading.stationName}{' '}
              <span className="text-sm text-gray-500">({reading.stationId})</span>
            </td>
            <td className="py-2 pr-4">{formatReading(reading.temperature)}</td>
            <td className="py-2 pr-4">{formatReading(reading.windGust)}</td>
            <td className="py-2 pr-4 text-sm text-gray-500">
              {formatMeasuredAt(reading.temperature ?? reading.windGust)}
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}
