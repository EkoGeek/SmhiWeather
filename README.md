# SMHI Weather Readings

Fetches **Lufttemperatur** (parameter `1`) and **Byvind** (parameter `21`) from the
[SMHI open data API](https://opendata-download-metobs.smhi.se/), combines them into one dataset per
station, and serves it through a REST API with a small React frontend on top.

## Stack

- Backend: C#, .NET 10, ASP.NET Core Web API (Minimal APIs), xUnit
- Frontend: React, TypeScript, Vite, Tailwind CSS

## Project structure

```text
backend/
  src/
    SmhiWeather.Domain/          domain model (WeatherStationReading, ParameterReading)
    SmhiWeather.Application/     ISmhiClient abstraction, WeatherReadingCombiner (merge logic), WeatherReadingsService
    SmhiWeather.Infrastructure/  SmhiClient - HttpClient-based SMHI API client + JSON DTOs
    SmhiWeather.Api/             Minimal API endpoint, API key auth, composition root
  tests/
    SmhiWeather.UnitTests/       WeatherReadingCombiner + WeatherReadingsService tests (no network calls)
    SmhiWeather.IntegrationTests/ full HTTP pipeline via WebApplicationFactory, SMHI client stubbed out
frontend/
  src/
    api/          fetch wrapper + types
    hooks/        useWeatherReadings (loading/error/success state)
    components/   WeatherFilters, WeatherTable
```

`SmhiWeather.Api` depends on `Application` and `Infrastructure`; `Infrastructure` depends on
`Application` and `Domain`; `Application` depends on `Domain` only. `ISmhiClient` lives in
`Application` so the combining logic can be unit tested against a fake, without any HTTP dependency.

## Running locally

### Backend

```powershell
dotnet run --project backend/src/SmhiWeather.Api
```

Runs on `https://localhost:7131` (see `backend/src/SmhiWeather.Api/Properties/launchSettings.json`).
No SMHI API key is required - their open data endpoints are unauthenticated.

**This API's own endpoints require an API key**, sent as an `X-Api-Key` header. For local development
the key is a plain value in `appsettings.json` (`ApiKey:Key`, defaults to `local-dev-demo-key`) - per
the project's requirements, there's no need to store this securely; it exists only to prove the
request-validation requirement, not as a real secret.

```powershell
curl -H "X-Api-Key: local-dev-demo-key" "https://localhost:7131/api/weather-readings"
```

OpenAPI schema is available at `/openapi/v1.json` in development for discoverability.

### Frontend

```powershell
cd frontend
npm install
cp .env.example .env.local   # sets VITE_API_KEY to match the backend's default dev key
npm run dev
```

Runs on `http://localhost:5173` and proxies `/api/*` to the backend (`vite.config.ts`). Open it in a
browser - filter by station ID and/or period, or leave blank for all stations / latest hour.

## The API

`GET /api/weather-readings`

| Query param | Values | Default |
|---|---|---|
| `stationId` | an SMHI station id, e.g. `98230` | all stations |
| `period` | `hour` \| `day` | `hour` |

No parameters set -> all stations, latest hour. Returns `400` with a validation problem for an
unrecognized `period`. Every request requires a valid `X-Api-Key` header, or `401`.

Response is a flat array combining both parameters per station; either `temperature` or `windGust`
may be `null` if that station doesn't report it:

```json
[
  {
    "stationId": "98230",
    "stationName": "Stockholm-Observatoriekullen A",
    "latitude": 59.3417,
    "longitude": 18.0549,
    "temperature": { "value": 16.8, "unit": "celsius", "measuredAt": "2026-08-16T11:00:00+00:00", "quality": "G" },
    "windGust": null
  }
]
```

## Testing

```powershell
dotnet test SmhiWeather.slnx
```

- `WeatherReadingCombinerTests` - the core "combine two datasets" logic: merges matching stations,
  keeps a station present in only one dataset (with the other parameter `null`), and handles the
  empty case. Pure unit tests, no I/O.
- `WeatherReadingsServiceTests` - proves the service fetches both parameters via `ISmhiClient` and
  forwards `stationId`/`period` correctly, using a hand-written fake client.
- `SmhiWeather.IntegrationTests` - exercises the real HTTP pipeline (auth, validation, routing) via
  `WebApplicationFactory`, with `ISmhiClient` replaced by a stub so tests never depend on SMHI being
  reachable.

```powershell
cd frontend && npm run test
```

## Design notes / known limitations

- SMHI's single-station endpoint (`.../station/{id}/...`) and its all-stations endpoint
  (`.../station-set/all/...`) return **different JSON shapes** (confirmed against the live API) -
  handled with two separate DTOs in `Infrastructure/Smhi/Dtos`.
- A `404` from SMHI for one parameter (e.g. a station that doesn't measure wind gust) is treated as
  "no data" for that parameter, not an error - the other parameter is still returned.
- The all-stations call fetches every station on every request; there's no caching. Fine for this
  exercise, but a real deployment would want a short-lived cache given SMHI only updates hourly data.
- The API key is a single static value, not per-client keys or a real identity provider - matches the
  stated requirement that no key needs to be safely stored here.
