using System.Text.Json.Nodes;

namespace CITToFirmCSharp.Migrators;

public class ArmorItemMigrator : Migrator
{
    private static string? GetTexture(Dictionary<string, string> properties)
    {
        var helmet = properties.GetValueOrDefault("texture.leather_helmet");
        var chestplate = properties.GetValueOrDefault("texture.leather_chestplate");
        var leggings = properties.GetValueOrDefault("texture.leather_leggings");
        var boots = properties.GetValueOrDefault("texture.leather_boots");
        return helmet ?? chestplate ?? leggings ?? boots;
    }
    /// <inheritdoc />
    public override bool Migrate(string id, Dictionary<string, string> properties, string originalPath)
    {
        var texture = GetTexture(properties);
        if (texture == null) return false;
        if(texture.Contains("dyed")) return false;

        var jsonObject = new JsonObject
        {
            ["parent"] = "minecraft:item/generated",
            ["textures"] = new JsonObject
            {
                ["layer0"] = $"hypixelplus:item/{texture}"
            },
            ["firmament:tint_overrides"] = new JsonObject
            {
                ["0"] = -1
            }
        };

        File.WriteAllText(Path.Combine(ModelOutputPath, $"{id.ToLowerInvariant()}.json"), jsonObject.ToString());

        var texturePath = Path.Combine(Path.GetDirectoryName(originalPath)!, Path.ChangeExtension(texture, ".png"));
        var textureOutputPath = Path.Combine(TextureOutputPath, Path.GetFileName(texturePath));
        if (!File.Exists(textureOutputPath) && File.Exists(texturePath))
        {
            File.Move(texturePath, textureOutputPath);
        }

        return true;
    }
}