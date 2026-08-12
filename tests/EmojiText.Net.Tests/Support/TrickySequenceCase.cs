using System.Text.RegularExpressions;

namespace EmojiText.Net.Tests.Support;

/// <summary>
/// A single named case from the tricky-sequences fixture: source text plus
/// the exact emoji matches expected within it, each given as a UTF-16 start
/// index and a space-separated hex code point sequence that the matched
/// substring must decode to.
/// </summary>
public sealed record TrickySequenceCase(string Name, string Text, IReadOnlyList<(int Index, string HexCodePoints)> ExpectedMatches)
{
    private const string NamePrefix = "NAME ";
    private const string TextPrefix = "TEXT ";
    private const string MatchPrefix = "MATCH ";
    private const string RawCodeUnitPattern = @"\\u\{([0-9A-Fa-f]{4})\}";

    internal static IEnumerable<TrickySequenceCase> ReadAll(string fixturePath)
    {
        var blocks = File.ReadAllText(fixturePath)
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split("\n\n", StringSplitOptions.RemoveEmptyEntries);

        foreach (var block in blocks)
        {
            yield return Parse(block);
        }
    }

    private static TrickySequenceCase Parse(string block)
    {
        var lines = block.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var name = lines[0][NamePrefix.Length..];
        var text = UnescapeRawCodeUnits(lines[1][TextPrefix.Length..]);

        var matches = new List<(int, string)>();
        foreach (var line in lines.Skip(2))
        {
            var payload = line[MatchPrefix.Length..];
            var separatorIndex = payload.IndexOf(' ');
            var index = int.Parse(payload[..separatorIndex]);
            var hexCodePoints = payload[(separatorIndex + 1)..];
            matches.Add((index, hexCodePoints));
        }

        return new TrickySequenceCase(name, text, matches);
    }

    private static string UnescapeRawCodeUnits(string text) =>
        Regex.Replace(text, RawCodeUnitPattern, match => ((char)Convert.ToInt32(match.Groups[1].Value, 16)).ToString());
}
