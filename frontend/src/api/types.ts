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
  /** One entry for period=hour, up to ~24 for period=day. Empty if the station doesn't measure it. */
  temperature: ParameterReading[]
  windGust: ParameterReading[]
}

export type Period = 'hour' | 'day'

export interface WeatherReadingsQuery {
  stationId?: string
  period?: Period
}
