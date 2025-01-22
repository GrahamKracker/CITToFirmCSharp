using System.Text.Json.Nodes;

namespace CITToFirmCSharp.Migrators;

public class BowMigrator : Migrator
{
    private Tuple<string, string, string, string>? GetTextures(Dictionary<string, string> properties)
    {
        var @base = properties.GetValueOrDefault("texture.bow");
        var pulling0 = properties.GetValueOrDefault("texture.bow_pulling_0");
        var pulling1 = properties.GetValueOrDefault("texture.bow_pulling_1");
        var pulling2 = properties.GetValueOrDefault("texture.bow_pulling_2");
        if (@base == null || pulling0 == null || pulling1 == null || pulling2 == null) return null;
        return Tuple.Create(@base, pulling0, pulling1, pulling2);
    }

    private string CreateBowModel(string id, string path, int state)
    {
        return $"{{\"parent\": \"firmskyblock:item/{id}\",\"textures\": {{\"layer0\": \"hypixelplus:item/{path}_pulling_{state}\"}}}}";
    }

    /// <inheritdoc />
    public override bool Migrate(string id, Dictionary<string, string> properties, string originalPath)
    {
        if (GetTextures(properties) == null) return false;

        var jsonObject = new JsonObject
        {
            ["parent"] = "minecraft:item/bow",
            ["textures"] = new JsonObject
            {
                ["layer0"] = $"hypixelplus:item/{Path.GetFileNameWithoutExtension(originalPath)}"
            },
            ["overrides"] = new JsonArray
            {
                new JsonObject
                {
                    ["predicate"] = new JsonObject
                    {
                        ["pulling"] = 1
                    },
                    ["model"] = $"firmskyblock:item/{id}_pulling_0"
                },
                new JsonObject
                {
                    ["predicate"] = new JsonObject
                    {
                        ["pulling"] = 1,
                        ["pull"] = 0.65
                    },
                    ["model"] = $"firmskyblock:item/{id}_pulling_1"
                },
                new JsonObject
                {
                    ["predicate"] = new JsonObject
                    {
                        ["pulling"] = 1,
                        ["pull"] = 0.9
                    },
                    ["model"] = $"firmskyblock:item/{id}_pulling_2"
                }
            }
        };

        File.WriteAllText(Path.Combine(ModelOutputPath, $"{id}.json"), jsonObject.ToString());
        File.WriteAllText(Path.Combine(ModelOutputPath, $"{id}_pulling_0.json"), CreateBowModel(id, Path.GetFileNameWithoutExtension(originalPath), 0));
        File.WriteAllText(Path.Combine(ModelOutputPath, $"{id}_pulling_1.json"), CreateBowModel(id, Path.GetFileNameWithoutExtension(originalPath), 1));
        File.WriteAllText(Path.Combine(ModelOutputPath, $"{id}_pulling_2.json"), CreateBowModel(id, Path.GetFileNameWithoutExtension(originalPath), 2));

        var baseTexturePath = Path.Combine(Path.GetDirectoryName(originalPath)!, $"{Path.GetFileNameWithoutExtension(originalPath)}.png");
        var pulling0TexturePath = Path.Combine(Path.GetDirectoryName(originalPath)!, $"{Path.GetFileNameWithoutExtension(originalPath)}_pulling_0.png");
        var pulling1TexturePath = Path.Combine(Path.GetDirectoryName(originalPath)!, $"{Path.GetFileNameWithoutExtension(originalPath)}_pulling_1.png");
        var pulling2TexturePath = Path.Combine(Path.GetDirectoryName(originalPath)!, $"{Path.GetFileNameWithoutExtension(originalPath)}_pulling_2.png");
        var baseTextureOutputPath = Path.Combine(TextureOutputPath, $"{Path.GetFileNameWithoutExtension(originalPath)}.png");
        var pulling0TextureOutputPath = Path.Combine(TextureOutputPath, $"{Path.GetFileNameWithoutExtension(originalPath)}_pulling_0.png");
        var pulling1TextureOutputPath = Path.Combine(TextureOutputPath, $"{Path.GetFileNameWithoutExtension(originalPath)}_pulling_1.png");
        var pulling2TextureOutputPath = Path.Combine(TextureOutputPath, $"{Path.GetFileNameWithoutExtension(originalPath)}_pulling_2.png");

        if (!File.Exists(baseTextureOutputPath) && File.Exists(baseTexturePath))
        {
            File.Move(baseTexturePath, baseTextureOutputPath, true);
        }
        if (!File.Exists(pulling0TextureOutputPath) && File.Exists(pulling0TexturePath))
        {
            File.Move(pulling0TexturePath, pulling0TextureOutputPath, true);
        }
        if (!File.Exists(pulling1TextureOutputPath) && File.Exists(pulling1TexturePath))
        {
            File.Move(pulling1TexturePath, pulling1TextureOutputPath, true);
        }
        if (!File.Exists(pulling2TextureOutputPath) && File.Exists(pulling2TexturePath))
        {
            File.Move(pulling2TexturePath, pulling2TextureOutputPath, true);
        }

        return true;
    }
}