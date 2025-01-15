using MosaicoSolutions.CSharpProperties;

namespace CITToFirmCSharp.Migrators;

public abstract class Migrator
{
    public abstract bool CanMigrate(Properties properties, string originalPath);
    public abstract void Migrate(Properties properties, string originalPath);
}