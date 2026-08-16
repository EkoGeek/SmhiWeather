export interface ParameterReading {
  value: number
  unit: string
  measuredAt: string
  quality: string
}

export interface WeatherStationReading {
  stationId: string
  stationName: string
  latitude: number | null
  longitude: number | null
  temperature: ParameterReading | null
  windGust: ParameterReading | null
}

export type Period = 'hour' | 'day'

export interface WeatherReadingsQuery {
  stationId?: string
  period?: Period
}
