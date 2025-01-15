using MosaicoSolutions.CSharpProperties;

namespace CITToFirmCSharp.Migrators;

public class SimpleItemMigrator : Migrator
{
    /// <inheritdoc />
    public override bool CanMigrate(Properties properties, string originalPath)
    {
        return false;
    }

    /// <inheritdoc />
    public override void Migrate(Properties properties, string originalPath)
    {

    }
}