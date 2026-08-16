import type { WeatherReadingsQuery, WeatherStationReading } from './types'

// Fine to expose for local development only - see README. Never do this for a real API key.
const API_KEY = import.meta.env.VITE_API_KEY as string | undefined

export class WeatherApiError extends Error {}

export async function fetchWeatherReadings(
  query: WeatherReadingsQuery,
  signal?: AbortSignal,
): Promise<WeatherStationReading[]> {
  const params = new URLSearchParams()
  if (query.stationId) params.set('stationId', query.stationId)
  if (query.period) params.set('period', query.period)

  const response = await fetch(`/api/weather-readings?${params.toString()}`, {
    headers: API_KEY ? { 'X-Api-Key': API_KEY } : {},
    signal,
  })

  if (!response.ok) {
    const problem = await response.json().catch(() => null)
    throw new WeatherApiError(problem?.title ?? `Request failed with status ${response.status}.`)
  }

  return (await response.json()) as WeatherStationReading[]
}
