using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using CITToFirmCSharp.Migrators;

namespace CITToFirmCSharp;

public class ItemMigrator
{
    private static string[] _blacklistedProperties = new[] { "weight" };

    private readonly List<Migrator> _migrators = Assembly.GetExecutingAssembly().GetTypes()
        .Where(t => t.IsSubclassOf(typeof(Migrator)))
        .Select(t => (Migrator)Activator.CreateInstance(t)!).OrderBy(m => m.Priority).ToList();

    public async Task Run()
    {
        await ItemList.SetItemIds();

        Console.WriteLine("Beginning migrating items...");
        var itemsStopWatch = Stopwatch.StartNew();

        var citPath = Path.Combine(TempPath, "assets", "minecraft", "optifine", "cit");
        var skyblockPath = Path.Combine(citPath, "skyblock");
        Directory.Delete(Path.Combine(citPath, "bedwars"), true);
        Directory.Delete(Path.Combine(citPath, "housing"), true);
        Directory.Delete(Path.Combine(citPath, "murder"), true);
        File.Delete(Path.Combine(citPath, "fix.properties"));

        var modelsPath = Path.Combine(TempPath, "assets", "minecraft", "models", "item");
        foreach (var file in Directory.GetFiles(modelsPath, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            var newPath = Path.Combine(NewPath, "assets", "minecraft", "models", "item", Path.GetFileName(file));
            Directory.CreateDirectory(Path.GetDirectoryName(newPath)!);
            File.Move(file, newPath);
        }

        File.Move(Path.Combine(TempPath, "assets", "minecraft", "optifine", "cit", "ui", "achoo.png"), Path.Combine(
            TextureOutputPath, "achoo.png"));

        foreach (var file in Directory.GetFiles(skyblockPath, "*.properties", SearchOption.AllDirectories))
        {
            try
            {
                var propertyStream = File.Open(file, FileMode.Open, FileAccess.Read);
                var properties = await PropertiesParser.ParsePropertiesAsync(propertyStream);
                propertyStream.Close();
                properties = properties.Where(p => !_blacklistedProperties.Contains(p.Key)).ToDictionary(p => p.Key, p => p.Value);

                var customId = properties.GetValueOrDefault("components.custom_data.id", string.Empty);
                var ids = GetPossibleItems(customId);
                if (ids is [])
                {
                    continue;
                }

                foreach (var id in ids)
                {
                    // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                    // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
                    foreach (var migrator in _migrators)
                    {
                        try
                        {
                            if (migrator.Migrate(id.ToLowerInvariant(), properties, file))
                            {
                                if(!CleanUp) //seems counter-intuitive but this ensures that only the unhandled files remain in the temp directory
                                    File.Delete(file);
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error migrating item: " + id + ": " +
                                              Path.GetRelativePath(TempPath, file));
                            Console.WriteLine(e);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        foreach (var file in Directory.GetFiles(skyblockPath, "*.mcmeta", SearchOption.AllDirectories))
        {
            var animatedFile = Path.Combine(Path.GetDirectoryName(file)!, Path.GetFileName(file));
            if (File.Exists(animatedFile))
                File.Move(animatedFile, Path.Combine(TextureOutputPath, Path.GetFileName(animatedFile)));
        }

        itemsStopWatch.Stop();
        Console.WriteLine("Items migrated in " + itemsStopWatch.ElapsedMilliseconds + "ms");
    }


    private static string[] GetPossibleItems(string input)
    {
        if (input.StartsWith("regex:"))
        {
            var regex = new Regex($"^{input[6..]}$");
            return ItemList.ItemIds.Where(id => regex.IsMatch(id)).ToArray();
        }
        if (ItemList.ItemIds.Contains(input)) {
            return [input];
        }

        return [];
    }

    public static void UpdateMaxSupportedFormat()
    {
        const int maxSupportedFormat = 42;
        var maxSupportedFormatFile = Path.Combine(NewPath, "pack.mcmeta");

        var maxSupportedFormatJson = JsonNode.Parse(File.ReadAllText(maxSupportedFormatFile));
        maxSupportedFormatJson?["pack"]?["pack_format"]?.ReplaceWith(maxSupportedFormat);

        if (maxSupportedFormatJson != null)
        {
            File.WriteAllText(maxSupportedFormatFile, maxSupportedFormatJson.ToString());
        }
    }
}