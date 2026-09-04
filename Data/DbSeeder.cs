using GamerBacklog.Domain.Entities;
using GamerBacklog.Domain.Enums;
using GamerBacklog.Integrations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GamerBacklog.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var config = services.GetRequiredService<IConfiguration>();
        var catalog = services.GetRequiredService<ICatalogClient>();

        // 1) Games (RAWG when configured; built-in demo catalog otherwise, and as fallback on failure)
        if (!await db.Games.AnyAsync())
        {
            var games = await catalog.GetPopularGamesAsync(40);
            if (games.Count == 0)
            {
                var demoCatalog = services.GetService<DemoIgdbClient>();
                if (demoCatalog != null)
                {
                    games = await demoCatalog.GetPopularGamesAsync(24);
                }
            }
            db.Games.AddRange(games);
            await db.SaveChangesAsync();
        }

        var gamesList = await db.Games.ToListAsync();
        var byName = gamesList.ToDictionary(g => g.Name, g => g, StringComparer.OrdinalIgnoreCase);
        Game? Find(string name) => byName.TryGetValue(name, out var g) ? g : null;

        // 1b) Real Metacritic scores for known games (covers demo-mode games; RAWG sync refreshes the rest)
        var metacriticByGame = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Elden Ring"] = 96,
            ["The Witcher 3: Wild Hunt"] = 92,
            ["God of War Ragnarök"] = 94,
            ["Hades"] = 93,
            ["Hollow Knight"] = 90,
            ["Stardew Valley"] = 89,
            ["Cyberpunk 2077"] = 76,
            ["Red Dead Redemption 2"] = 93,
            ["Baldur's Gate 3"] = 96,
            ["The Legend of Zelda: Breath of the Wild"] = 97,
            ["Super Mario Odyssey"] = 97,
            ["Persona 5 Royal"] = 95,
            ["Sekiro: Shadows Die Twice"] = 90,
            ["Bloodborne"] = 92,
            ["DOOM Eternal"] = 88,
            ["Portal 2"] = 95,
            ["Celeste"] = 91,
            ["Disco Elysium"] = 91,
            ["Terraria"] = 83,
            ["It Takes Two"] = 88,
            ["Marvel's Spider-Man"] = 87,
            ["Horizon Zero Dawn"] = 89,
            ["Metroid Dread"] = 88,
            ["Minecraft"] = 93,
        };
        var mcTouched = false;
        foreach (var g in gamesList)
        {
            if (g.Metacritic == null && metacriticByGame.TryGetValue(g.Name, out var mc))
            {
                g.Metacritic = mc;
                mcTouched = true;
            }
        }
        if (mcTouched) await db.SaveChangesAsync();

        // 2) Demo achievements for the first 12 games
        if (!await db.Achievements.AnyAsync())
        {
            var achievements = new (string Name, string Desc, double Rarity)[]
            {
                ("First Steps", "Complete the game's prologue.", 85),
                ("Marking Territory", "Complete 10 main objectives.", 60),
                ("Victor", "Finish the main campaign.", 42),
                ("No Escape", "Win a hard optional challenge.", 30),
                ("Collector", "Find 50 collectibles.", 22),
                ("Local Legend", "Complete every side quest.", 12),
                ("Perfectionist", "Reach 100% completion.", 6),
                ("Journey's End", "See every possible ending.", 2),
            };

            foreach (var g in gamesList.Take(12))
            {
                for (var i = 0; i < achievements.Length; i++)
                {
                    db.Achievements.Add(new Achievement
                    {
                        GameId = g.Id,
                        ExternalId = $"ach-{g.Id}-{i + 1}",
                        Name = achievements[i].Name,
                        Description = achievements[i].Desc,
                        RarityPercent = achievements[i].Rarity
                    });
                }
            }
            await db.SaveChangesAsync();
        }

        // 3) Demo users
        var demo = await EnsureUserAsync(userManager, "demo", "demo@gamerbacklog.dev", "Demo123!",
            "Demo", "Weekend gamer. RPGs and roguelikes are my vice — and the backlog keeps growing.");
        var ana = await EnsureUserAsync(userManager, "ana", "ana@gamerbacklog.dev", "Demo123!",
            "Ana Souza", "JRPG and indie fan. Platinum hunter in my spare time.");

        // 3b) Hidden admin panel account (role-gated /admin_panel; override via "Admin" config section)
        const string adminRole = "Admin";
        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(adminRole));
        }

        var adminUsername = config["Admin:Username"] ?? "admin";
        var admin = await userManager.FindByNameAsync(adminUsername);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = adminUsername,
                Email = $"{adminUsername}@gamerbacklog.dev",
                DisplayName = "Admin",
                EmailConfirmed = true
            };
            var adminPassword = config["Admin:Password"] ?? "GbAdmin!2026";
            var adminResult = await userManager.CreateAsync(admin, adminPassword);
            if (!adminResult.Succeeded)
            {
                throw new InvalidOperationException("Failed to create admin user: " +
                    string.Join("; ", adminResult.Errors.Select(e => e.Description)));
            }
        }
        if (!await userManager.IsInRoleAsync(admin, adminRole))
        {
            await userManager.AddToRoleAsync(admin, adminRole);
        }

        // 4) Follows
        if (!await db.Follows.AnyAsync())
        {
            db.Follows.Add(new Follow { FollowerId = demo.Id, FolloweeId = ana.Id, CreatedAt = DaysAgo(5) });
            db.Follows.Add(new Follow { FollowerId = ana.Id, FolloweeId = demo.Id, CreatedAt = DaysAgo(4) });
            await db.SaveChangesAsync();
        }

        // 5) Libraries + activities
        if (!await db.UserGames.AnyAsync())
        {
            await AddShelfAsync(db, demo.Id, Find("Elden Ring"), PlayStatus.Playing, 5, null, 62, 1);
            await AddShelfAsync(db, demo.Id, Find("Hades"), PlayStatus.Played, 5,
                "Escaping the Underworld has never been this addictive. Razor-sharp combat and a brilliant narrative structure.", 84, 12);
            await AddShelfAsync(db, demo.Id, Find("Hollow Knight"), PlayStatus.Playing, 4, null, 27, 2);
            await AddShelfAsync(db, demo.Id, Find("The Witcher 3: Wild Hunt"), PlayStatus.Played, 4,
                "Brilliant expansions, main story drags a bit in the middle. In the end, worth every hour.", 97, 30);
            await AddShelfAsync(db, demo.Id, Find("Stardew Valley"), PlayStatus.Backlog, null, null, null, 20);
            await AddShelfAsync(db, demo.Id, Find("Cyberpunk 2077"), PlayStatus.Abandoned, 2,
                "Came back after the patch and still crashed three times. Maybe someday I'll get back to it.", 15, 40);
            await AddShelfAsync(db, demo.Id, Find("Baldur's Gate 3"), PlayStatus.Playing, 5, null, 41, 1);
            await AddShelfAsync(db, demo.Id, Find("Celeste"), PlayStatus.Wishlist, null, null, null, 15);
            await AddShelfAsync(db, demo.Id, Find("DOOM Eternal"), PlayStatus.Played, 4, null, 14, 50);

            await AddShelfAsync(db, ana.Id, Find("Persona 5 Royal"), PlayStatus.Played, 5,
                "Impeccable style, engaging story. 110 hours that flew by.", 110, 8);
            await AddShelfAsync(db, ana.Id, Find("God of War Ragnarök"), PlayStatus.Played, 5,
                "The Norse saga wraps up in style. Atreus has grown so much.", 45, 6);
            await AddShelfAsync(db, ana.Id, Find("Elden Ring"), PlayStatus.Playing, 4, null, 88, 3);
            await AddShelfAsync(db, ana.Id, Find("Hades"), PlayStatus.Backlog, null, null, null, 18);
            await AddShelfAsync(db, ana.Id, Find("Celeste"), PlayStatus.Played, 5,
                "Perfectly hard, with a huge heart. The final chapter destroyed me.", 22, 25);
        }

        // 6) Demo unlocked achievements
        if (!await db.UserAchievements.AnyAsync())
        {
            await UnlockAsync(db, demo.Id, Find("Hades")?.Id ?? 0, 5);
            await UnlockAsync(db, demo.Id, Find("The Witcher 3: Wild Hunt")?.Id ?? 0, 4);
            await UnlockAsync(db, ana.Id, Find("Elden Ring")?.Id ?? 0, 6);
            await UnlockAsync(db, ana.Id, Find("Celeste")?.Id ?? 0, 3);
        }

        // 7) Notifications for the demo user
        if (!await db.Notifications.AnyAsync())
        {
            db.Notifications.Add(new Notification
            {
                UserId = demo.Id,
                Text = "Ana Souza started following you.",
                Link = "/u/ana",
                IsRead = false,
                CreatedAt = DaysAgo(4)
            });

            var persona = Find("Persona 5 Royal");
            if (persona != null)
            {
                db.Notifications.Add(new Notification
                {
                    UserId = demo.Id,
                    Text = "Ana Souza reviewed Persona 5 Royal.",
                    Link = $"/Games/{persona.Id}",
                    IsRead = false,
                    CreatedAt = DaysAgo(8)
                });
            }

            var gow = Find("God of War Ragnarök");
            if (gow != null)
            {
                db.Notifications.Add(new Notification
                {
                    UserId = demo.Id,
                    Text = "Ana Souza marked God of War Ragnarök as Played.",
                    Link = $"/Games/{gow.Id}",
                    IsRead = true,
                    CreatedAt = DaysAgo(6)
                });
            }

            await db.SaveChangesAsync();
        }
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager, string username, string email, string password,
        string displayName, string bio)
    {
        var existing = await userManager.FindByNameAsync(username);
        if (existing != null) return existing;

        var user = new ApplicationUser
        {
            UserName = username,
            Email = email,
            DisplayName = displayName,
            Bio = bio,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Failed to create demo user: " +
                string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        return user;
    }

    private static async Task AddShelfAsync(
        AppDbContext db, string userId, Game? game, PlayStatus status, int? rating,
        string? review, int? hours, int daysAgo)
    {
        if (game == null) return;
        var user = await db.Users.FindAsync(userId);
        var name = string.IsNullOrWhiteSpace(user?.DisplayName) ? user?.UserName : user!.DisplayName;

        db.UserGames.Add(new UserGame
        {
            UserId = userId,
            GameId = game.Id,
            Status = status,
            Rating = rating,
            Review = review,
            HoursPlayed = hours,
            CreatedAt = DaysAgo(daysAgo),
            UpdatedAt = DaysAgo(daysAgo)
        });

        if (!string.IsNullOrWhiteSpace(review))
        {
            db.Activities.Add(new Activity
            {
                UserId = userId,
                GameId = game.Id,
                Type = ActivityType.Review,
                Text = $"{name} rated {game.Name} {rating ?? 0} star{((rating ?? 0) == 1 ? "" : "s")}.",
                CreatedAt = DaysAgo(daysAgo)
            });
        }
        else
        {
            db.Activities.Add(new Activity
            {
                UserId = userId,
                GameId = game.Id,
                Type = ActivityType.Status,
                Text = $"{name} marked {game.Name} as {status.Label()}.",
                CreatedAt = DaysAgo(daysAgo)
            });
        }

        await db.SaveChangesAsync();
    }

    private static async Task UnlockAsync(AppDbContext db, string userId, int gameId, int count)
    {
        if (gameId <= 0) return;
        var game = await db.Games.FindAsync(gameId);
        var user = await db.Users.FindAsync(userId);
        var name = string.IsNullOrWhiteSpace(user?.DisplayName) ? user?.UserName : user!.DisplayName;

        var achievements = await db.Achievements
            .Where(a => a.GameId == gameId)
            .OrderByDescending(a => a.RarityPercent)
            .Take(count)
            .ToListAsync();

        var i = 0;
        foreach (var a in achievements)
        {
            db.UserAchievements.Add(new UserAchievement
            {
                UserId = userId,
                AchievementId = a.Id,
                UnlockedAt = DaysAgo(Math.Max(1, 20 - i))
            });
            i++;
        }

        db.Activities.Add(new Activity
        {
            UserId = userId,
            GameId = gameId,
            Type = ActivityType.Achievement,
            Text = $"{name} unlocked {achievements.Count} achievement{(achievements.Count == 1 ? "" : "s")} in {game!.Name}.",
            CreatedAt = DaysAgo(10)
        });

        await db.SaveChangesAsync();
    }

    private static DateTime DaysAgo(int days) => DateTime.UtcNow.AddDays(-days);
}
