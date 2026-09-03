using GamerBacklog.Domain.Entities;

namespace GamerBacklog.Integrations;

/// <summary>
/// Built-in demo catalog client with ~24 popular real games.
/// Covers are generated locally via the /covers/{slug} endpoint.
/// </summary>
public class DemoIgdbClient : ICatalogClient
{
    public Task<IReadOnlyList<Game>> GetPopularGamesAsync(int take = 40)
    {
        IReadOnlyList<Game> games = Catalog.All;
        return Task.FromResult(games.Take(take).ToList() as IReadOnlyList<Game>);
    }

    public Task<Game?> GetByIdAsync(long igdbId)
    {
        return Task.FromResult(Catalog.All.FirstOrDefault(g => g.IgdbId == igdbId));
    }
}

internal static class Catalog
{
    public static readonly IReadOnlyList<Game> All = Build();

    private static List<Game> Build()
    {
        var rows = new (string Name, int Year, string Developer, string Genres, string Platforms, string Summary)[]
        {
            ("Elden Ring", 2022, "FromSoftware", "RPG,Action,Adventure", "PC,PS5,PS4,Xbox Series X|S,Xbox One",
                "A vast action RPG set in the Lands Between, created by FromSoftware and George R. R. Martin. Explore hidden dungeons, defeat demigods and become the Elden Lord."),
            ("The Witcher 3: Wild Hunt", 2015, "CD Projekt Red", "RPG,Adventure,Open World", "PC,PS5,PS4,Xbox One,Switch",
                "Geralt of Rivia hunts monsters and searches for his adopted daughter in one of the largest open-world RPGs ever made."),
            ("God of War Ragnarök", 2022, "Santa Monica Studio", "Action,Adventure", "PS5,PS4",
                "Kratos and Atreus face the end of times in Norse mythology in this epic action-adventure sequel."),
            ("Hades", 2020, "Supergiant Games", "Roguelike,Action,Indie", "PC,Switch,PS5,Xbox Series X|S",
                "An action roguelike where Zagreus, son of Hades, tries to escape the Underworld again and again — and the story advances with every attempt."),
            ("Hollow Knight", 2017, "Team Cherry", "Metroidvania,Platformer,Indie", "PC,Switch,PS4,Xbox One",
                "A hand-drawn metroidvania set in the ruins of the kingdom of Hallownest, full of secrets, bosses and somber beauty."),
            ("Stardew Valley", 2016, "ConcernedApe", "Simulation,RPG,Indie", "PC,Switch,PS4,Xbox One,Mobile",
                "You've inherited your grandfather's farm: plant, fish, mine and make friends in a charming life simulation in the countryside."),
            ("Cyberpunk 2077", 2020, "CD Projekt Red", "RPG,FPS,Open World", "PC,PS5,Xbox Series X|S",
                "An open-world action RPG in the megacity of Night City, where you play V, a mercenary chasing an immortal implant."),
            ("Red Dead Redemption 2", 2018, "Rockstar Games", "Action,Adventure,Open World", "PC,PS4,Xbox One",
                "The saga of Arthur Morgan and the Van der Linde gang at the twilight of the American old west."),
            ("Baldur's Gate 3", 2023, "Larian Studios", "RPG,Strategy", "PC,PS5,Xbox Series X|S",
                "An epic RPG based on D&D 5e, with choices that shape the story, memorable companions and tactical turn-based combat."),
            ("The Legend of Zelda: Breath of the Wild", 2017, "Nintendo", "Adventure,Action,Open World", "Switch,Wii U",
                "Link awakens from a 100-year sleep to save Hyrule in an open world of free and creative exploration."),
            ("Super Mario Odyssey", 2017, "Nintendo", "Platformer,Adventure", "Switch",
                "Mario travels the world with the hat Cappy, taking over enemies and objects to rescue Peach from a forced wedding."),
            ("Persona 5 Royal", 2019, "Atlus", "JRPG,Life Sim", "PS5,PS4,PC,Xbox Series X|S,Switch",
                "Students become Phantom Thieves and steal the hearts of the corrupt in Tokyo, in a stylish JRPG full of life."),
            ("Sekiro: Shadows Die Twice", 2019, "FromSoftware", "Action,Adventure", "PC,PS4,Xbox One",
                "A shinobi with a prosthetic arm seeks vengeance and rescues his lord in feudal Japan. Precision and posture above all."),
            ("Bloodborne", 2015, "FromSoftware", "RPG,Action,Horror", "PS4",
                "Hunters armed with saws and firearms face beasts and cosmic horrors in the gothic city of Yharnam."),
            ("DOOM Eternal", 2020, "id Software", "FPS,Action", "PC,PS5,PS4,Xbox Series X|S,Switch",
                "The Doom Slayer returns to massacre demons in a fast and aggressive FPS across an invaded Earth."),
            ("Portal 2", 2011, "Valve", "Puzzle,Platformer", "PC,PS3,Xbox 360,Switch",
                "Solve portal puzzles at Aperture Science, with the sarcastic AI GLaDOS and a brilliant co-op mode."),
            ("Celeste", 2018, "Maddy Makes Games", "Platformer,Indie", "PC,Switch,PS4,Xbox One",
                "Help Madeline climb Celeste Mountain in a precise and emotional platformer about anxiety and overcoming."),
            ("Disco Elysium", 2019, "ZA/UM", "RPG,Mystery", "PC,PS5,Xbox Series X|S,Switch",
                "A detective RPG with no combat, where your skills talk to each other and every dialogue shapes the city of Revachol."),
            ("Terraria", 2011, "Re-Logic", "Sandbox,Adventure", "PC,Switch,PS4,Xbox One,Mobile",
                "Dig, build and fight in an endlessly replayable 2D sandbox, alone or with friends."),
            ("It Takes Two", 2021, "Hazelight Studios", "Co-op,Adventure,Platformer", "PC,PS5,PS4,Xbox Series X|S,Switch",
                "An inventive co-op adventure where Cody and May, turned into dolls, must work together to save their marriage."),
            ("Marvel's Spider-Man", 2018, "Insomniac Games", "Action,Adventure,Open World", "PS5,PS4,PC",
                "Peter Parker balances personal life and heroism while swinging through a lively, detailed New York."),
            ("Horizon Zero Dawn", 2017, "Guerrilla Games", "RPG,Action,Open World", "PS4,PC",
                "Aloy hunts machine-animals in a beautiful post-apocalyptic world, uncovering the secrets of a distant past."),
            ("Metroid Dread", 2021, "Nintendo", "Metroidvania,Action", "Switch",
                "Samus faces the dreaded E.M.M.I. robots on hostile planets in this tense 2D return of the Metroid series."),
            ("Minecraft", 2011, "Mojang", "Sandbox,Survival", "PC,Switch,PS4,Xbox One,Mobile",
                "Build anything with blocks, explore endless caves and survive the night in the best-selling sandbox of all time."),
        };

        var list = new List<Game>();
        for (var i = 0; i < rows.Length; i++)
        {
            var r = rows[i];
            list.Add(new Game
            {
                IgdbId = i + 1,
                Name = r.Name,
                ReleaseYear = r.Year,
                Developer = r.Developer,
                Genres = r.Genres,
                Platforms = r.Platforms,
                Summary = r.Summary,
                CoverUrl = "/covers/" + Uri.EscapeDataString(r.Name)
            });
        }
        return list;
    }
}
