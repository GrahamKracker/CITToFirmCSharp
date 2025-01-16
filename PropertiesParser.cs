

using System.Runtime.CompilerServices;
using System.Text;

namespace CITToFirmCSharp;

public static class PropertiesParser
{
    public static async Task<Dictionary<string,string>> ParsePropertiesAsync(string path)
    {
        var properties = new Dictionary<string, string>();

        await using var fileStream = File.OpenRead(path);
        using var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, 4096);
        while (await streamReader.ReadLineAsync() is { } line)
        {
            if (!IsValidLine(line))
                continue;

            var parts = line.Split('=', 2);
            if (parts.Length != 2)
                continue;

            properties[parts[0]] = parts[1];
        }

        return properties;
    }

    private static bool IsValidLine(string line)
    {
        return !string.IsNullOrEmpty(line) && !StartsWithIgnoredCharacters(line);
    }

    private static bool StartsWithIgnoredCharacters(string line)
    {
        return IgnoredCharacters.Any(line.StartsWith);
    }

    private static readonly string[] IgnoredCharacters =
    [
        "'''",
        ";",
        "#"
    ];
}