import type { ParameterReading, WeatherStationReading } from '../api/types'

function formatValue(reading: ParameterReading): string {
  return `${reading.value} ${reading.unit}`
}

function formatTime(reading: ParameterReading): string {
  return new Date(reading.measuredAt).toLocaleString()
}

/** Shows the latest reading; if there's a series (period=day), the rest expand on demand. */
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
    <details>
      <summary className="cursor-pointer">
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
            Latest measured at
          </th>
        </tr>
      </thead>
      <tbody>
        {readings.map((reading) => {
          const latest = reading.temperature.at(-1) ?? reading.windGust.at(-1)

          return (
            <tr key={reading.stationId} className="border-b border-gray-100 align-top">
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
