
using Newtonsoft.Json.Linq;

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

        var jsonObject = new JObject
        {
            ["parent"] = "minecraft:item/generated",
            ["textures"] = new JObject
            {
                ["layer0"] = $"hypixelplus:item/{Path.GetFileNameWithoutExtension(originalPath)}"
            }
        };

        var generatedPath = Path.Combine(NewProgram.ModelOutputPath, $"{id.ToLowerInvariant()}.json");
        if (!File.Exists(generatedPath))
        {
            //todo: maybe save all the jsons and write them at the end to avoid file race conditions that i am ignoring here :)
            try
            {
                using var file = File.Open(generatedPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                file.SetLength(0);
                using var writer = new StreamWriter(file);
                writer.Write(jsonObject.ToString());
            }
            catch (Exception e)
            {
                return true;
            }
        }

        var texturePath = Path.ChangeExtension(originalPath, ".png");
        var textureOutputPath = Path.Combine(NewProgram.TextureOutputPath, Path.GetFileName(texturePath));
        if(!File.Exists(textureOutputPath))
        {
            File.Move(texturePath, textureOutputPath);
        }

        return true;
    }
}