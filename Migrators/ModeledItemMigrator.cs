using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace CITToFirmCSharp.Migrators;

public partial class ModeledItemMigrator : Migrator
{
    private Tuple<string, string?> getModelTexture(Dictionary<string, string> properties)
    {
        var model = properties.GetValueOrDefault("model");
        if (model == null) return Tuple.Create((string)null!, (string)null!)!;
        var texture = properties.GetValueOrDefault("texture");

        return Tuple.Create(model, texture);
    }

    /// <inheritdoc />
    public override bool Migrate(string id, Dictionary<string, string> properties, string originalPath)
    {
        (string model, string texture) = getModelTexture(properties);
        if (model == null) return false;

        if (properties.Count is 1 or > 4) return false; //todo: dont hardcode the amount of properties
        if (texture != null)
        {
            var jObject = new JObject
            {
                ["parent"] = model,
                ["textures"] = new JObject
                {
                    ["layer0"] = $"hypixelplus:item/{texture}"
                }
            };
            var modelFile =
                Path.Combine(ModelOutputPath, $"{id.ToLowerInvariant()}.json");
            File.WriteAllText(modelFile, jObject.ToString());

            var texturePath = Path.ChangeExtension(originalPath, ".png");
            var textureOutputPath = Path.Combine(TextureOutputPath, Path.GetFileName(texturePath));
            if (!File.Exists(textureOutputPath) && File.Exists(texturePath))
            {
                File.Move(texturePath, textureOutputPath);
            }
        }
        else
        {
            //Console.WriteLine("Modeled item migration: " + id + ": " + Path.GetRelativePath(TempPath, originalPath));

            var modelFile = new FileInfo(Path.Combine(Path.GetDirectoryName(originalPath)!, $"{model}.json"));
            if (modelFile.Exists)
            {
                var text = File.ReadAllText(modelFile.FullName);

                var textures = JsonNode.Parse(text)?["textures"]?.AsObject();
                if (textures == null)
                {
                    File.WriteAllText(Path.Combine(ModelOutputPath, $"{id.ToLowerInvariant()}.json"), text);
                    return true;
                }

                foreach (var relativeTexture in JsonNode.Parse(text)["textures"].AsObject().Select(x => x.Value.ToString()!).ToList())
                {
                    var fileName = relativeTexture.Substring(relativeTexture.LastIndexOf("/") + 1);
                    if(!relativeTexture.StartsWith("./"))
                        continue;

                    var texturePath = relativeTexture[2..];
                    var textureOutputPath = Path.Combine(TextureOutputPath, $"{fileName}.png");
                    if (File.Exists(Path.Combine(Path.GetDirectoryName(modelFile.FullName)!, $"{texturePath}.png")))
                    {
                        File.Move(Path.Combine(Path.GetDirectoryName(modelFile.FullName)!, $"{texturePath}.png"), textureOutputPath);
                    }

                    text = text.Replace(relativeTexture, $"hypixelplus:item/{fileName}");
                }
                text = text.Replace("""
                                    "layer0": "optifine/cit/ui/achoo.png",
                                    """, """
                                         "layer0": "hypixelplus:item/achoo",
                                         """);
                File.WriteAllText(Path.Combine(ModelOutputPath, $"{id.ToLowerInvariant()}.json"), text);
            }
            else
            {

                var customModel = new JObject
                {
                    ["parent"] = "minecraft:item/generated",
                    ["textures"] = new JObject
                    {
                        ["layer0"] = model
                    }
                };
                Console.WriteLine($"Model file not found for {id}: {modelFile.FullName}");

                File.WriteAllText(modelFile.FullName, customModel.ToString());
                
                return false;
            }

        }

        return true;
    }
}