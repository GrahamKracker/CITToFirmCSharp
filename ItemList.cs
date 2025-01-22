using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CITToFirmCSharp;

public class ItemList
{
    public static readonly HashSet<string> ItemIds =
    [
        "ARCHER_DUNGEON_ABILITY_1",
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
        "BINGONIMBUS_2000",
        "ROOKIE_SPADE",
        "BINGHOE",
        "CACTUS_KNIFE_2",
        "CACTUS_KNIFE_3",
        "COCO_CHOPPER_2",
        "COCO_CHOPPER_3",
        "HOE_OF_NO_TILLING",
        "ABIPHONE_FLIP_DRAGON",
        "ABIPHONE_FLIP_NUCLEUS",
        "ABIPHONE_FLIP_VOLCANO",
        "ATOMSPLIT_KATANA",
        "BINGO_COMBAT_TALISMAN",
        "MAGIC_8_BALL",

    ];

    public static async Task SetItemIds()
    {
        var client = new HttpClient();
        var itemListStopwatch = Stopwatch.StartNew();

        if (File.Exists(ItemIdsPath))
        {
            var file = File.Open(ItemIdsPath, FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(file);
            while (!reader.EndOfStream)
            {
                ItemIds.Add(await reader.ReadLineAsync() ?? string.Empty);
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
            if (response == null)
                throw new ApplicationException("Failed to get a valid response from the Hypixel API");

            var items = response["items"]?.AsArray();
            if (items == null)
                throw new ApplicationException("Failed to get item list from the Hypixel API");

            foreach (var item in items.AsArray())
            {
                var itemId = item?["id"]?.ToString();
                if (itemId == null)
                    continue;
                ItemIds.Add(itemId);
                await stream.WriteLineAsync(itemId);
            }

            file.Close();
        }

        itemListStopwatch.Stop();
        await Console.Out.WriteLineAsync("Item list loaded in " + itemListStopwatch.ElapsedMilliseconds + "ms");
    }
}