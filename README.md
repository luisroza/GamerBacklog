# GameBacklog

A Blazor Web App prototype for a game library and backlog command center.

## What is implemented

- Dashboard with playing games, priority shortlist, and quick picks.
- Library search, status/platform filters, tags, ratings, and inline status updates.
- Backlog assistant that recommends what to play based on mood, time, and platform.
- Stats page focused on momentum, platforms, status distribution, and completion.
- Modular .NET structure:
  - `GameBacklog.Domain`
  - `GameBacklog.Application`
  - `GameBacklog.Infrastructure`
  - `GameBacklog.Web`

The current data source is an in-memory seeded service so the UX can be explored immediately.

## Run locally

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\start-dev.ps1
```

Then open:

```text
http://localhost:5188
```

## Build

This environment works best with single-node MSBuild because parallel project builds can lock intermediate files.

```powershell
$env:DOTNET_CLI_HOME="C:\Users\rozam\OneDrive\Documentos\New project\.dotnet-home"
$env:APPDATA="C:\Users\rozam\OneDrive\Documentos\New project\.appdata"
$env:LOCALAPPDATA="C:\Users\rozam\OneDrive\Documentos\New project\.localappdata"
dotnet build src\GameBacklog.Web\GameBacklog.Web.csproj --configfile NuGet.Config -p:MSBuildEnableWorkloadResolver=false -m:1
```

## Next milestones

- Replace the in-memory service with EF Core and PostgreSQL.
- Add ASP.NET Core Identity for user-owned libraries.
- Add an external metadata provider such as IGDB or RAWG.
- Add imports, starting with CSV or Steam.
- Add tests for filtering, status updates, and pick-next recommendation rules.
