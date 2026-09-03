# GamerBacklog

A social game catalog and backlog tracker inspired by [Backloggd](https://backloggd.com/) — track what you're playing, rate and review games, follow friends, and sync Steam achievements.

Built with **ASP.NET Core (MVC + Razor)**, **EF Core + SQLite**, **ASP.NET Core Identity** and **TailwindCSS**.

## Features

- **Accounts & public profiles** — sign up, avatar, bio, stats, shelves by status (`/u/{username}`)
- **Game catalog** — search, genre/platform filters, pagination; game pages with cover, summary, achievements and community reviews
- **Backlog statuses** — Want to Play / Playing / Played / Abandoned / Wishlist
- **Ratings & reviews** — 1–5 stars + text reviews, per-game community average
- **Library** — tabs by status with counts, inline status change, remove
- **Social** — follow/unfollow, activity feed, notifications
- **Integrations**:
  - **RAWG** (free API) — imports top-rated games with their official **Metacritic scores** (`Sync catalog now` button)
  - **Steam** — real achievement sync when `Steam:ApiKey` is configured (simulated otherwise)
  - **PSN / Xbox** — stubs ("coming soon")

## Getting started

```bash
dotnet run --project GamerBacklog
```

The SQLite database is created, migrated and seeded automatically on first boot.

**Demo account:** `demo@GamerBacklog.dev` / `Demo123!` (also `ana@gamerbacklog.dev` / `Demo123!`)

## Configuration (optional API keys)

```json
{
  "Rawg": { "ApiKey": "" },
  "Steam": { "ApiKey": "" }
}
```

- **RAWG key** (free, ~20k requests/month): register at <https://rawg.io/apidocs>. Games are imported **on demand**: when a user searches for a game that isn't in the local database yet, it is fetched from RAWG (with its Metacritic score, cover, genres and platforms) and stored locally — the API is never called twice for the same game; every search afterwards hits the database only. Per-game details (description/developer) are also fetched lazily, at most once per game, when its page is first opened. A small initial list of top-rated games is seeded on first boot. Without a key, the built-in 24-game demo catalog is used and the site runs fully offline.
- **Steam key** (<https://steamcommunity.com/dev>): enables real achievement/library sync via the Steam Web API; without it, the sync button runs in simulated mode (30–90% unlock).

## Project structure

```
GamerBacklog/
├── Controllers/        # MVC controllers (Games, Library, Profile, Settings, ...)
├── Data/               # AppDbContext, migrations, seeder
├── Domain/
│   ├── Entities/       # Game, UserGame, Achievement, Follow, ...
│   └── Enums/          # PlayStatus (+ labels/colors helpers)
├── Integrations/       # RawgClient, SteamClient, PSN/Xbox stubs, ICatalogClient
├── Models/             # ViewModels
├── Services/           # Library, Follow, Notification, covers (SVG), UI helpers
├── Views/              # Razor views
└── wwwroot/            # Static assets (css, generated covers/avatars)
```

## Notes

- Game covers/avatars are deterministic SVGs generated locally (`/covers/{slug}`, `/avatar/{username}`); RAWG images are used when available with local fallback.
- PSN and Xbox have no official public APIs; their integrations are stubbed for a future release.
