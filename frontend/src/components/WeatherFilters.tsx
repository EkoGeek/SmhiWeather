import type { FormEvent } from 'react'
import type { Period } from '../api/types'

interface WeatherFiltersProps {
  stationId: string
  period: Period
  onChange: (next: { stationId: string; period: Period }) => void
}

export function WeatherFilters({ stationId, period, onChange }: WeatherFiltersProps) {
  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const form = new FormData(event.currentTarget)
    onChange({
      stationId: String(form.get('stationId') ?? '').trim(),
      period: form.get('period') === 'day' ? 'day' : 'hour',
    })
  }

  return (
    <form onSubmit={handleSubmit} className="flex flex-wrap items-end gap-4">
      <div className="flex flex-col gap-1">
        <label htmlFor="stationId" className="text-sm font-medium text-gray-700">
          Station ID
        </label>
        <input
          id="stationId"
          name="stationId"
          type="text"
          defaultValue={stationId}
          placeholder="All stations"
          className="rounded border border-gray-300 px-3 py-1.5 focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500"
        />
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
