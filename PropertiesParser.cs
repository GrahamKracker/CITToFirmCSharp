using System.Runtime.CompilerServices;
using System.Text;

namespace CITToFirmCSharp;

public static class PropertiesParser
{
    public static async Task<Dictionary<string,string>> ParsePropertiesAsync(FileStream path)
    {
        var properties = new Dictionary<string, string>();

        using var streamReader = new StreamReader(path, Encoding.UTF8, true, 4096);
        while (await streamReader.ReadLineAsync() is { } line)
        {
            if (!IsValidLine(line))
                continue;

            var parts = line.Split('=', 2);

            properties[parts[0]] = parts[1];
        }

        return properties;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidLine(string line)
    {
        return !string.IsNullOrEmpty(line) && !StartsWithIgnoredCharacters(line);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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