import { useRef, useState, type FormEvent } from 'react'
import type { Period } from '../api/types'

interface WeatherFiltersProps {
  stationId: string
  period: Period
  onChange: (next: { stationId: string; period: Period }) => void
}

export function WeatherFilters({ stationId, period, onChange }: WeatherFiltersProps) {
  const [stationIdValue, setStationIdValue] = useState(stationId)
  const stationIdInputRef = useRef<HTMLInputElement>(null)

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const form = new FormData(event.currentTarget)
    onChange({
      stationId: String(form.get('stationId') ?? '').trim(),
      period: form.get('period') === 'day' ? 'day' : 'hour',
    })
  }

  function handleClearStationId() {
    setStationIdValue('')
    stationIdInputRef.current?.focus()
    // Force period back to 'hour': SMHI has no all-stations feed for 'day', so keeping it would
    // just produce an empty/error result instead of the "all stations" view the user wants.
    onChange({ stationId: '', period: 'hour' })
  }

  return (
    <form onSubmit={handleSubmit} className="flex flex-wrap items-end gap-4">
      <div className="flex flex-col gap-1">
        <label htmlFor="stationId" className="text-sm font-medium text-gray-700">
          Station ID
        </label>
        <div className="relative">
          <input
            ref={stationIdInputRef}
            id="stationId"
            name="stationId"
            type="text"
            value={stationIdValue}
            onChange={(event) => setStationIdValue(event.target.value)}
            placeholder="All stations"
            className="rounded border border-gray-300 py-1.5 pl-3 pr-8 focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500"
          />
          {stationIdValue && (
            <button
              type="button"
              onClick={handleClearStationId}
              aria-label="Clear station ID"
              className="absolute inset-y-0 right-2 flex items-center text-gray-400 hover:text-gray-600 focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500"
            >
              ×
            </button>
          )}
        </div>
      </div>

      <div className="flex flex-col gap-1">
        <label htmlFor="period" className="text-sm font-medium text-gray-700">
          Period
        </label>
        <select
          id="period"
          name="period"
          defaultValue={period}
          className="rounded border border-gray-300 px-3 py-1.5 focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500"
        >
          <option value="hour">Latest hour</option>
          <option value="day">Latest day</option>
        </select>
      </div>

      <button
        type="submit"
        className="rounded bg-blue-600 px-4 py-1.5 font-medium text-white hover:bg-blue-700 focus-visible:ring-2 focus-visible:ring-blue-500 focus:outline-none"
      >
        Apply
      </button>
    </form>
  )
}
