import { useEffect, useState } from 'react'
import { fetchWeatherReadings, WeatherApiError } from '../api/weatherApi'
import type { WeatherReadingsQuery, WeatherStationReading } from '../api/types'

interface WeatherReadingsState {
  data: WeatherStationReading[]
  isLoading: boolean
  error: string | null
}

export function useWeatherReadings(query: WeatherReadingsQuery): WeatherReadingsState {
  const [state, setState] = useState<WeatherReadingsState>({
    data: [],
    isLoading: true,
    error: null,
  })

  useEffect(() => {
    const controller = new AbortController()

    setState((previous) => ({ ...previous, isLoading: true, error: null }))

    fetchWeatherReadings(query, controller.signal)
      .then((data) => setState({ data, isLoading: false, error: null }))
      .catch((error: unknown) => {
        if (controller.signal.aborted) return
        const message =
          error instanceof WeatherApiError ? error.message : 'Failed to load weather data.'
        setState({ data: [], isLoading: false, error: message })
      })

    return () => controller.abort()
  }, [query.stationId, query.period])

  return state
}
