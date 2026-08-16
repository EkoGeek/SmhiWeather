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
    const validationDetail = problem?.errors && Object.values(problem.errors).flat()[0]
    const message =
      validationDetail ?? problem?.title ?? `Request failed with status ${response.status}.`
    throw new WeatherApiError(String(message))
  }

  return (await response.json()) as WeatherStationReading[]
}
