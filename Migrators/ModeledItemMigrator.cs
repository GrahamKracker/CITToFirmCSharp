using System.Text.Json.Nodes;

namespace CITToFirmCSharp.Migrators;

public class ModeledItemMigrator : Migrator
{
    private static Tuple<string?, string?> GetModelTexture(Dictionary<string, string> properties)
    {
        var model = properties.GetValueOrDefault("model");
        if (model == null)
            return Tuple.Create<string?, string?>(null!, null!);
        var texture = properties.GetValueOrDefault("texture");

        return Tuple.Create(model, texture)!;
    }

    /// <inheritdoc />
    public override bool Migrate(string id, Dictionary<string, string> properties, string originalPath)
    {
        (string? model, string? texture) = GetModelTexture(properties);
        if (model == null) return false;

        if (texture != null)
        {
            JsonObject jObject;

            if (texture.StartsWith("textures/"))
            {
                texture = texture[9..];
                jObject = new JsonObject
                {
                    ["parent"] = model,
                    ["textures"] = new JsonObject
                    {
                        ["layer0"] = $"minecraft:{texture}"
                    }
                };
            }
            else jObject = new JsonObject
            {
                ["parent"] = model,
                ["textures"] = new JsonObject
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
            var modelFile = new FileInfo(Path.Combine(Path.GetDirectoryName(originalPath)!, Path.ChangeExtension(model, ".json")));
            if (modelFile.Exists)
            {
                var text = File.ReadAllText(modelFile.FullName);
                modelFile.Delete();

                var textures = JsonNode.Parse(text)?["textures"]?.AsObject();
                if (textures == null)
                {
                    File.WriteAllText(Path.Combine(ModelOutputPath, $"{id.ToLowerInvariant()}.json"), text);
                    return true;
                }

                foreach (var relativeTexture in textures.Select(x => x.Value?.ToString()!).ToList())
                {
                    var fileName = relativeTexture.Substring(relativeTexture.LastIndexOf('/') + 1);
                    if (!relativeTexture.StartsWith("./"))
                        continue;

                    var texturePath = relativeTexture[2..];
                    var textureOutputPath = Path.Combine(TextureOutputPath, $"{fileName}.png");
                    if (File.Exists(Path.Combine(Path.GetDirectoryName(modelFile.FullName)!, $"{texturePath}.png")) &&
                        !File.Exists(textureOutputPath))
                    {
                        File.Move(Path.Combine(Path.GetDirectoryName(modelFile.FullName)!, $"{texturePath}.png"),
                            textureOutputPath);
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
                JsonObject? customModel;

                if (model.StartsWith("textures/"))
                {
                    model = model[9..];
                    customModel = new JsonObject
                    {
                        ["parent"] = $"minecraft:{model}"
                    };
                }
                else if (model.StartsWith("item/"))
                {
                    model = model[5..];
                    customModel = new JsonObject
                    {
                        ["parent"] = $"minecraft:item/{model}"
                    };
                }
                else
                {
                    if (File.Exists(Path.Combine(ModelOutputPath, $"{model.ToLowerInvariant()}.json")))
                    {
                        customModel = JsonNode.Parse(File.ReadAllText(Path.Combine(ModelOutputPath, $"{model.ToLowerInvariant()}.json")))?.AsObject();
                    }
                    else
                    {
                        customModel = new JsonObject
                        {
                            ["parent"] = $"hypixelplus:item/{model}"
                        };
                    }
                }

                File.WriteAllText(Path.Combine(ModelOutputPath, $"{id.ToLowerInvariant()}.json"), customModel?.ToString());

                return false;
            }
        }

        return true;
    }
}