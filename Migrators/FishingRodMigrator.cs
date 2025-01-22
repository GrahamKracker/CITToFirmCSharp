using System.Text.Json.Nodes;

namespace CITToFirmCSharp.Migrators;

public class FishingRodMigrator : Migrator
{
    private Tuple<string, string>? GetTextures(Dictionary<string, string> properties)
    {
        var @base = properties.GetValueOrDefault("texture.fishing_rod");
        var cast = properties.GetValueOrDefault("texture.fishing_rod_cast");
        if (@base == null || cast == null) return null;
        return Tuple.Create(@base, cast);
    }

    /// <inheritdoc />
    public override bool Migrate(string id, Dictionary<string, string> properties, string originalPath)
    {
        if(GetTextures(properties) == null) return false;
        if (properties.Count != 4) return false;

        var jsonObject = new JsonObject
        {
            ["parent"] = "minecraft:item/handheld_rod",
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
                        ["cast"] = 1
                    },
                    ["model"] = $"firmskyblock:item/{Path.GetFileNameWithoutExtension(originalPath)}_cast"
                }
            }
        };

        var castJson = new JsonObject
        {
            ["parent"] = "minecraft:item/fishing_rod",
            ["textures"] = new JsonObject
            {
                ["layer0"] = $"hypixelplus:item/{Path.GetFileNameWithoutExtension(originalPath)}_cast"
            }
        };

        File.WriteAllText(Path.Combine(ModelOutputPath, $"{id}.json"), jsonObject.ToString());
        File.WriteAllText(Path.Combine(ModelOutputPath, $"{id}_cast.json"), castJson.ToString());

        var baseTexturePath = Path.Combine(Path.GetDirectoryName(originalPath)!, $"{Path.GetFileNameWithoutExtension(originalPath)}.png");
        var castTexturePath = Path.Combine(Path.GetDirectoryName(originalPath)!, $"{Path.GetFileNameWithoutExtension(originalPath)}_cast.png");
        var baseTextureOutputPath = Path.Combine(TextureOutputPath, $"{Path.GetFileNameWithoutExtension(originalPath)}.png");
        var castTextureOutputPath = Path.Combine(TextureOutputPath, $"{Path.GetFileNameWithoutExtension(originalPath)}_cast.png");
        if (!File.Exists(baseTextureOutputPath) && File.Exists(baseTexturePath))
        {
            File.Move(baseTexturePath, baseTextureOutputPath);
        }
        if (!File.Exists(castTextureOutputPath) && File.Exists(castTexturePath))
        {
            File.Move(castTexturePath, castTextureOutputPath, true);
        }

        return true;
    }
}