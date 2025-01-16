using System.Text.RegularExpressions;

namespace CITToFirmCSharp.Migrators;

public partial class NullMigrator : Migrator
{
    /// <inheritdoc />
    public override bool Migrate(string id, Dictionary<string, string> properties, string originalPath)
    {
        if (NullRegex().IsMatch(properties.GetValueOrDefault("components.custom_name", string.Empty)))
        {
            //delete filename.png if it exists
            var pngPath = Path.ChangeExtension(originalPath, ".png");
            if (File.Exists(pngPath))
            {
                File.Delete(pngPath);
            }
            return true;
        }
        return false;
    }

    [GeneratedRegex(".*null")]
    private partial Regex NullRegex();
}