namespace CITToFirmCSharp.Migrators;

public abstract class Migrator
{
    public abstract bool Migrate(string id, Dictionary<string, string> properties, string originalPath);

    public virtual int Priority => 0;
}