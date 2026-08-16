import { useState } from 'react'
import { WeatherFilters } from './components/WeatherFilters'
import { WeatherTable } from './components/WeatherTable'
import { useWeatherReadings } from './hooks/useWeatherReadings'
import type { Period } from './api/types'

function App() {
  const [filters, setFilters] = useState<{ stationId: string; period: Period }>({
    stationId: '',
    period: 'hour',
  })

  const { data, isLoading, error } = useWeatherReadings({
    stationId: filters.stationId || undefined,
    period: filters.period,
  })

  return (
    <main className="mx-auto max-w-4xl px-4 py-8">
      <h1 className="mb-1 text-2xl font-semibold text-gray-900">SMHI Weather Readings</h1>
      <p className="mb-6 text-gray-600">
        Combined Lufttemperatur and Byvind data from SMHI's open data API.
      </p>

      <WeatherFilters
        key={`${filters.stationId}-${filters.period}`}
        stationId={filters.stationId}
        period={filters.period}
        onChange={setFilters}
      />

      <div className="mt-6" aria-live="polite">
        {isLoading && <p className="text-gray-600">Loading...</p>}
        {error && !isLoading && <p className="text-red-700">{error}</p>}
        {!isLoading && !error && (
          <WeatherTable
            readings={data}
            onSelectStation={(stationId) => setFilters({ stationId, period: 'day' })}
          />
        )}
      </div>
    </main>
  )
}

export default App
