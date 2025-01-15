using System.Text.Json;
using System.Text.Json.Nodes;

namespace CITToFirmCSharp;

public class NewProgram
{
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

    private static async Task Main()
    {
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


    }
}