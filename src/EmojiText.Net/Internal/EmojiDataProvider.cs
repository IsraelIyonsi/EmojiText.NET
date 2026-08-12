using System.Globalization;
using System.Reflection;

namespace EmojiText.Internal;

/// <summary>
/// Loads the embedded, pinned Unicode RGI emoji sequence table and builds
/// the <see cref="EmojiSequenceTrie"/> used to scan text.
/// </summary>
internal static class EmojiDataProvider
{
    private const string ResourceName = "EmojiText.EmojiSequences.dat";
    private const char CommentPrefix = '#';
    private const string VersionCommentPrefix = "# unicode-emoji-version:";
    private const char CodePointSeparator = ' ';
    private const NumberStyles CodePointNumberStyle = NumberStyles.HexNumber;

    private static readonly Lazy<EmojiDataSet> LazyData = new(Load);

    /// <summary>
    /// Gets the trie built from the embedded RGI emoji sequence table.
    /// </summary>
    internal static EmojiSequenceTrie Trie => LazyData.Value.Trie;

    /// <summary>
    /// Gets the Unicode emoji version declared in the embedded data file,
    /// used by the stale-guard test to confirm it matches
    /// <see cref="Emoji.UnicodeEmojiVersion"/>.
    /// </summary>
    internal static string DataVersion => LazyData.Value.Version;

    private static EmojiDataSet Load()
    {
        using var stream = typeof(EmojiDataProvider).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Embedded emoji data resource '{ResourceName}' was not found.");
        using var reader = new StreamReader(stream);

        string? version = null;
        var sequences = new List<int[]>();

        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (line.Length == 0)
            {
                continue;
            }

            if (line[0] == CommentPrefix)
            {
                if (line.StartsWith(VersionCommentPrefix, StringComparison.Ordinal))
                {
                    version = line[VersionCommentPrefix.Length..].Trim();
                }

                continue;
            }

            sequences.Add(ParseCodePoints(line));
        }

        if (version is null)
        {
            throw new InvalidOperationException($"Embedded emoji data resource '{ResourceName}' is missing its version header.");
        }

        return new EmojiDataSet(new EmojiSequenceTrie(sequences), version);
    }

    private static int[] ParseCodePoints(string line)
    {
        var tokens = line.Split(CodePointSeparator);
        var codePoints = new int[tokens.Length];
        for (var i = 0; i < tokens.Length; i++)
        {
            codePoints[i] = int.Parse(tokens[i], CodePointNumberStyle, CultureInfo.InvariantCulture);
        }

        return codePoints;
    }

    private readonly record struct EmojiDataSet(EmojiSequenceTrie Trie, string Version);
}
