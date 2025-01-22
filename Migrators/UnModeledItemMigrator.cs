
using System.Text.Json.Nodes;

namespace CITToFirmCSharp.Migrators;

public class UnModeledItemMigrator : Migrator
{
    /// <inheritdoc />
    public override bool Migrate(string id, Dictionary<string, string> properties, string originalPath)
    {
        //check if properties is just 'items' and 'components.custom_data.id', nothing else
        if (properties.Count != 2 || !properties.ContainsKey("items") || !properties.ContainsKey("components.custom_data.id"))
        {
            return false;
        }

        var jsonObject = new JsonObject
        {
            ["parent"] = "minecraft:item/generated",
            ["textures"] = new JsonObject
            {
                ["layer0"] = $"hypixelplus:item/{Path.GetFileNameWithoutExtension(originalPath)}"
            }
        };

        var generatedPath = Path.Combine(Program.ModelOutputPath, $"{id.ToLowerInvariant()}.json");
        if (!File.Exists(generatedPath))
        {
                using var file = File.Open(generatedPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                file.SetLength(0);
                using var writer = new StreamWriter(file);
                writer.Write(jsonObject.ToString());
        }

        var texturePath = Path.ChangeExtension(originalPath, ".png");
        var textureOutputPath = Path.Combine(Program.TextureOutputPath, Path.GetFileName(texturePath));
        if(!File.Exists(textureOutputPath) && File.Exists(texturePath))
        {
            File.Move(texturePath, textureOutputPath);
        }

        return true;
    }
}