using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using CITToFirmCSharp.Migrators;
using MosaicoSolutions.CSharpProperties;

namespace CITToFirmCSharp;

public class NewProgram
{
    private const string BasePath = @"C:\Users\gjguz\source\repos\Minecraft\CITToFirmCSharp\";
    private static readonly string OriginalPath = Path.Combine(BasePath, @"Hypixel+ 0.20.7 for 1.21.1\");
    private static readonly string NewPath = Path.Combine(BasePath, @"New Hypixel+\");
    private static readonly string VanillaPath = Path.Combine(BasePath, @"Vanilla Resource Pack\");
    private static readonly string BackupPath = Path.Combine(BasePath, @"New Hypixel+ Original Backup\");

    private readonly HashSet<string> _itemIds = [
        "ARCHER_DUNGEON_ABILITY_2",
        "ARCHER_DUNGEON_ABILITY_2",
        "ARCHER_DUNGEON_ABILITY_3",

        // Special Items
        "BURNING_COINS",
        "EXPENSIVE_TOY",

        // Idk?
        "SMALL_RUNES_SACK",
        "MEDIUM_RUNES_SACK",
        "LARGE_RUNES_SACK",
        "RIFT_TROPHY_MOUNTAIN",
        "DEAD_CAT_FOOD",
        "PRE_DIGESTION_FISH",
        "CHOCOLATE_CHIP",
        "PERFECT_HOPPER",
    ];

    private List<Migrator> _migrators = Assembly.GetExecutingAssembly().GetTypes()
        .Where(t => t.IsSubclassOf(typeof(Migrator)))
        .Select(t => (Migrator) Activator.CreateInstance(t))
        .ToList();

    private static async Task Main(string[] args)
    {
        if (Directory.Exists(NewPath))
        {
            Directory.Delete(NewPath, true);
        }
        Directory.CreateDirectory(NewPath);

        if (Directory.Exists(BackupPath))
        {
            Directory.Delete(BackupPath, true);
        }

        Directory.CreateDirectory(BackupPath);

        foreach (var file in Directory.GetFiles(OriginalPath, "*", SearchOption.AllDirectories))
        {
            var relativePath = file[OriginalPath.Length..];
            var newPath = Path.Combine(BackupPath, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(newPath)!);
            File.Copy(file, newPath, true);
        }

        await new NewProgram().Run();
    }

    private async Task Run()
    {
        var client = new HttpClient();
        var response =
            JsonSerializer.Deserialize<JsonObject>(
                await client.GetStringAsync("https://api.hypixel.net/v2/resources/skyblock/items"));
        foreach (var item in response["items"].AsArray())
        {
            _itemIds.Add(item["id"].ToString());
        }

        var citPath = Path.Combine(BackupPath, "assets", "minecraft", "optifine", "cit");
        Directory.Delete(Path.Combine(citPath, "bedwars"), true);
        Directory.Delete(Path.Combine(citPath, "housing"), true);
        Directory.Delete(Path.Combine(citPath, "murder"), true);
        File.Delete(Path.Combine(citPath, "fix.properties"));

        var modelsPath = Path.Combine(OriginalPath, "assets", "minecraft", "models", "item");

        foreach (var file in Directory.GetFiles(modelsPath, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            File.Copy(file, Path.Combine(NewPath, "assets", "minecraft", "models", "item"), false);
        }
        
        foreach (var migrator in _migrators)
        {
            foreach (var file in Directory.GetFiles(citPath, "*.properties", SearchOption.AllDirectories))
            {
                var properties = (Properties) await Properties.LoadAsync(file);
                if (migrator.CanMigrate(properties, file))
                {
                    migrator.Migrate(properties, file);
                }
            }
        }
    }
}