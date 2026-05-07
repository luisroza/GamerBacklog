# GameBacklog

A Blazor Web App prototype for a game library and backlog command center.

## Implemented

- Dashboard, library, backlog assistant, stats, architecture, and account pages.
- In-memory register/login/logout prototype.
- Manual game entry with status, ownership, platform, hours, and notes.
- Metadata provider abstraction with mock data and IGDB support.
- Catalog search abstraction for Valve Steam and PlayStation 5 / PSN.
- Steam owned/recently-played and PlayStation placeholder provider seams.

## Run

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\start-dev.ps1
```

Open `http://localhost:5188`.

## Build

```powershell
dotnet build src\GameBacklog.Web\GameBacklog.Web.csproj --configfile NuGet.Config -p:MSBuildEnableWorkloadResolver=false -m:1
```

## Notes

Configure `Igdb:ClientId` and `Igdb:ClientSecret` to use IGDB. Without credentials, metadata search uses the mock provider. Steam catalog search uses Valve's public app list endpoint with fallback sample data. PlayStation 5 search uses the configured metadata provider because PSN has no stable public full-catalog API for third-party apps.
