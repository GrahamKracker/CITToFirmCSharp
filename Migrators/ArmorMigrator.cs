using System.Text.Json.Nodes;

namespace CITToFirmCSharp.Migrators;

public class ArmorMigrator : Migrator
{
    private Tuple<string, int>? GetEquipmentPath(string id)
    {
        if (id.EndsWith("boots")) return Tuple.Create("humanoid", 1);
        if (id.EndsWith("leggings")) return Tuple.Create("humanoid_leggings", 2);
        if (id.EndsWith("chestplate")) return Tuple.Create("humanoid", 1);
        if (id.EndsWith("helmet")) return Tuple.Create("humanoid", 1);
        return null;
    }


    /// <inheritdoc />
    public override bool Migrate(string id, Dictionary<string, string> properties, string originalPath)
    {
        var equipmentPath = GetEquipmentPath(id);
        if (equipmentPath == null) return false;
        (string path, int layer) = equipmentPath;
        string[] armorType = ["leather", "iron", "gold", "diamond", "chainmail"];
        string? texture = null;
        string property = "";
        foreach (var type in armorType)
        {
            property = $"texture.{type}_layer_{layer}";
            texture = properties.GetValueOrDefault(property);
            if (texture != null) break;
        }

        if (texture == null) return false;

        var rlPath = texture.EndsWith("_layer_2") ? texture.Substring(0, texture.Length - 8) : texture.EndsWith("_layer_1") ? texture.Substring(0, texture.Length - 8) : texture;

        if(rlPath.Contains("dyed")) return false;

        var overlay = properties.GetValueOrDefault($"{property}_overlay");


        var jsonObject = new JsonObject
        {
            ["item_ids"] = new JsonArray { id.ToUpperInvariant() },
            ["layers"] = new JsonArray
            {
                new JsonObject
                {
                    ["identifier"] = $"hypixelplus:{rlPath}"
                }
            }
        };

        if (overlay != null && overlay != texture)
        {
            jsonObject["layers"]!.AsArray().Add(new JsonObject
            {
                ["identifier"] = $"hypixelplus:{rlPath}",
                ["suffix"] = "_overlay"
            });
        }

        File.WriteAllText(Path.Combine(ArmorOutputPath, $"{id.ToLowerInvariant()}.json"), jsonObject.ToString());

        var texturePath = Path.Combine(Path.GetDirectoryName(originalPath)!, Path.ChangeExtension(texture, ".png"));
        var textureOutputPath = Path.Combine(ArmorTextureOutputPath, path, rlPath + ".png");
        if (!Directory.Exists(Path.GetDirectoryName(textureOutputPath)!))
            Directory.CreateDirectory(Path.GetDirectoryName(textureOutputPath)!);
        if (!File.Exists(textureOutputPath) && File.Exists(texturePath))
        {
            File.Move(texturePath, textureOutputPath);
        }

        if (overlay != null && overlay != texture)
        {
            var overlayPath = Path.Combine(Path.GetDirectoryName(originalPath)!, Path.ChangeExtension(overlay, ".png"));
            var overlayOutputPath = Path.Combine(ArmorTextureOutputPath, path, $"{rlPath}_overlay.png");
            if (!File.Exists(overlayOutputPath) && File.Exists(overlayPath))
            {
                File.Move(overlayPath, overlayOutputPath);
            }
        }

        var animatedFile = Path.Combine(Path.GetDirectoryName(originalPath)!,
                Path.GetFileNameWithoutExtension(texture) + ".png.mcmeta");
        if (File.Exists(animatedFile))
            File.Copy(animatedFile, Path.Combine(ArmorTextureOutputPath, Path.GetFileName(animatedFile)));

        return true;
    }

    /// <inheritdoc />
    public override int Priority => -100;
}