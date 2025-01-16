using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using CITToFirmCSharp.Migrators;

namespace CITToFirmCSharp;

public class NewProgram
{
    private const string BasePath = @"C:\Users\gjguz\source\repos\Minecraft\CITToFirmCSharp\";
    private static readonly string OriginalPath = Path.Combine(BasePath, @"Hypixel+ 0.20.7 for 1.21.1\");
    private static readonly string NewPath = Path.Combine(BasePath, @"New Hypixel+\");
    private static readonly string VanillaPath = Path.Combine(BasePath, @"Vanilla Resource Pack\");
    private static readonly string BackupPath = Path.Combine(BasePath, @"New Hypixel+ Original Backup\");
    private static readonly string ItemIdsPath = Path.Combine(BasePath, "itemIds.json");

    public static readonly string ModelOutputPath = Path.Combine(NewPath, "assets", "firmskyblock", "models", "item");
    public static readonly string TextureOutputPath = Path.Combine(NewPath, "assets", "hypixelplus", "textures", "item");
    public static readonly string TextureArmorOutputPath = Path.Combine(NewPath, "assets", "hypixelplus", "textures", "entity", "equipment");
    public static readonly string ArmorOutputPath = Path.Combine(NewPath, "assets", "firmskyblock", "overrides", "armor_models");

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

    private readonly List<Migrator> _migrators = Assembly.GetExecutingAssembly().GetTypes()
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

        Directory.CreateDirectory(ModelOutputPath);
        Directory.CreateDirectory(TextureOutputPath);
        Directory.CreateDirectory(TextureArmorOutputPath);
        Directory.CreateDirectory(ArmorOutputPath);

        foreach (var file in Directory.GetFiles(OriginalPath, "*", SearchOption.AllDirectories))
        {
            var relativePath = file[OriginalPath.Length..];
            var newPath = Path.Combine(BackupPath, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(newPath)!);
            File.Copy(file, newPath, true);
        }

        await new NewProgram().Run();
    }

    private async Task SetItemIds()
    {
        var client = new HttpClient();
        var itemListStopwatch = Stopwatch.StartNew();

        if (File.Exists(ItemIdsPath))
        {
            var file = File.Open(ItemIdsPath, FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(file);
            while (!reader.EndOfStream)
            {
                _itemIds.Add(await reader.ReadLineAsync() ?? string.Empty);
            }

            file.Close();
        }
        else
        {
            var file = File.Open(ItemIdsPath, FileMode.Create, FileAccess.Write);
            var response =
                JsonSerializer.Deserialize<JsonObject>(
                    await client.GetStringAsync("https://api.hypixel.net/v2/resources/skyblock/items"));
            var stream = new StreamWriter(file);
            foreach (var item in response["items"].AsArray())
            {
                var itemId = item["id"].ToString();
                _itemIds.Add(itemId);
                await stream.WriteLineAsync(itemId);
            }

            file.Close();
        }

        itemListStopwatch.Stop();
        await Console.Out.WriteLineAsync("Item list loaded in " + itemListStopwatch.ElapsedMilliseconds + "ms");
    }

    private async Task Run()
    {
        await SetItemIds();

        var citPath = Path.Combine(BackupPath, "assets", "minecraft", "optifine", "cit");
        Directory.Delete(Path.Combine(citPath, "bedwars"), true);
        Directory.Delete(Path.Combine(citPath, "housing"), true);
        Directory.Delete(Path.Combine(citPath, "murder"), true);
        File.Delete(Path.Combine(citPath, "fix.properties"));

        var modelsPath = Path.Combine(OriginalPath, "assets", "minecraft", "models", "item");

        var modelsStopWatch = Stopwatch.StartNew();
        foreach (var file in Directory.GetFiles(modelsPath, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            var newPath = Path.Combine(NewPath, "assets", "minecraft", "models", "item", Path.GetFileName(file));
            Directory.CreateDirectory(Path.GetDirectoryName(newPath)!);
            File.Copy(file, newPath, true);
        }

        modelsStopWatch.Stop();
        await Console.Out.WriteLineAsync("Models moved in " + modelsStopWatch.ElapsedMilliseconds + "ms");

        var propertiesStopWatch = Stopwatch.StartNew();
        var tasks = new List<Task>();
        Parallel.ForEach(Directory.GetFiles(citPath, "*.properties", SearchOption.AllDirectories), file =>
        {
            tasks.Add(PropertiesParser.ParsePropertiesAsync(file).ContinueWith(task =>
            {
                var properties = task.Result;
                var customId = properties.GetValueOrDefault("components.custom_data.id", string.Empty);

                var ids = GetPossibleItems(customId);
                if (ids.Length == 0)
                {
                    /*await Console.Error.WriteLineAsync("Invalid id: " + customId);
                    await Console.Error.WriteLineAsync("path: " + file);*/
                    Migrate(string.Empty, properties, file);
                    return;
                }

                foreach (var id in ids)
                {
                    Migrate(id, properties, file);
                }
            }));
        });
        await Task.WhenAll(tasks);
        propertiesStopWatch.Stop();

        await Console.Out.WriteLineAsync("Properties migrated in " + propertiesStopWatch.ElapsedMilliseconds + "ms");
        return;

        void Migrate(string id, Dictionary<string, string> properties, string file)
        {

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var migrator in _migrators)
            {
                //log all properties
                if (properties.ContainsValue("null_acacia_log"))
                {
                    Console.WriteLine("null_acacia_log");
                }

                if (migrator.Migrate(id.ToLowerInvariant(), properties, file))
                {
                    break;
                }
            }
        }
    }


    private string[] GetPossibleItems(string input)
    {
        if (input.StartsWith("regex:"))
        {
            var regex = new Regex(input[6..]);
            return _itemIds.Where(id => regex.IsMatch(id)).ToArray();
        }
        if (_itemIds.Contains(input)) {
            return [input];
        }

        return [];
    }
}