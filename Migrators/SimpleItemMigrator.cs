
namespace CITToFirmCSharp.Migrators;

public class SimpleItemMigrator : Migrator
{
    /// <inheritdoc />
    public override bool Migrate(string id, Dictionary<string, string> properties, string originalPath)
    {
        if (properties.Count != 1) return false;
        var generatedJson =
            """{"parent": "minecraft:item/generated","textures": {"layer0": "hypixelplus:item/${path.fileNameExtless()}"}}""";
        var generatedPath = Path.Combine(NewProgram.ModelOutputPath, $"{id.ToLowerInvariant()}.json");
        File.WriteAllText(generatedPath, generatedJson);

        var texturePath = Path.ChangeExtension(originalPath, ".png");
        var textureOutputPath = Path.Combine(NewProgram.TextureOutputPath, Path.GetFileName(texturePath));
        if(!File.Exists(textureOutputPath))
        {
            File.Copy(texturePath, textureOutputPath);
        }

        return true;
    }
}