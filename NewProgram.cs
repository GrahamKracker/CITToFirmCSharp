using System.Text.Json;
using System.Text.Json.Nodes;

namespace CITToFirmCSharp;

public class NewProgram
{
    private static readonly HashSet<string> _itemIds = new();

    private async static Task Main(string[] args)
    {
        var client = new HttpClient();
        var response = JsonSerializer.Deserialize<JsonObject[]>(await client.GetStringAsync("https://api.hypixel.net/v2/resources/skyblock/items"));
        foreach (var item in response)
        {
            _itemIds.Add(item["id"].ToString());
        }
        await new NewProgram().Run();
    }

    public async Task Run()
    {
        Console.WriteLine("Hello World!");
    }
}