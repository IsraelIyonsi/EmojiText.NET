using System.Text;
using EmojiText.Internal;

namespace EmojiText;

/// <summary>
/// Emoji-aware text processing: detects, counts, strips, and replaces emoji
/// as the user-perceived units they are, not as raw UTF-16 chars or Unicode
/// scalar values. Matching is driven entirely by an embedded, pinned table
/// of RGI emoji sequences and does not depend on runtime globalization data.
/// </summary>
public static class Emoji
{
    private const char WhitespaceCollapseChar = ' ';

    /// <summary>
    /// Gets the pinned Unicode emoji specification version (see
    /// <see href="https://unicode.org/reports/tr51/">UTS #51</see>) that the
    /// embedded emoji sequence table was generated from.
    /// </summary>
    public const string UnicodeEmojiVersion = "16.0";

    /// <summary>
    /// Determines whether <paramref name="text"/> contains at least one RGI
    /// emoji sequence.
    /// </summary>
    /// <param name="text">The text to inspect, or <see langword="null"/>.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="text"/> contains an emoji;
    /// otherwise <see langword="false"/>. Returns <see langword="false"/> for
    /// <see langword="null"/> or empty input.
    /// </returns>
    public static bool HasEmoji(string? text) => EnumerateMatches(text).Any();

    /// <summary>
    /// Counts the RGI emoji sequences in <paramref name="text"/>, where each
    /// ZWJ sequence, flag, keycap, or skin-tone modified emoji counts as one
    /// match regardless of how many code points it spans.
    /// </summary>
    /// <param name="text">The text to inspect, or <see langword="null"/>.</param>
    /// <returns>
    /// The number of emoji matches. Returns 0 for <see langword="null"/> or
    /// empty input.
    /// </returns>
    public static int Count(string? text) => EnumerateMatches(text).Count();

    /// <summary>
    /// Enumerates every RGI emoji match in <paramref name="text"/>, in
    /// left-to-right order and without overlap.
    /// </summary>
    /// <param name="text">The text to scan, or <see langword="null"/>.</param>
    /// <returns>
    /// A lazily evaluated sequence of matches. Empty for <see langword="null"/>
    /// or empty input.
    /// </returns>
    public static IEnumerable<EmojiMatch> EnumerateMatches(string? text) =>
        EmojiScanner.Scan(text, EmojiDataProvider.Trie);

    /// <summary>
    /// Removes every emoji from <paramref name="text"/>.
    /// </summary>
    /// <param name="text">The text to strip, or <see langword="null"/>.</param>
    /// <param name="collapseWhitespace">
    /// When <see langword="true"/>, whitespace that touched a removed emoji
    /// (immediately before it, immediately after it, or both, once adjacent
    /// runs merge) is collapsed to a single space, or removed entirely when
    /// that leaves it at the very start or end of the result. Whitespace
    /// elsewhere in <paramref name="text"/> that never bordered an emoji is
    /// left exactly as it was. When <see langword="false"/> (the default),
    /// no whitespace is touched at all, which commonly leaves doubled spaces
    /// where an emoji used to be.
    /// </param>
    /// <returns>
    /// <paramref name="text"/> with all emoji removed. Returns an empty
    /// string for <see langword="null"/> or empty input.
    /// </returns>
    public static string Strip(string? text, bool collapseWhitespace = false)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        if (!collapseWhitespace)
        {
            return Replace(text, string.Empty);
        }

        var removalSites = new List<int>();
        var removedSoFar = 0;
        var stripped = ReplaceEach(text, match =>
        {
            removalSites.Add(match.Index - removedSoFar);
            removedSoFar += match.Length;
            return string.Empty;
        });

        return CollapseWhitespaceAroundRemovals(stripped, removalSites);
    }

    /// <summary>
    /// Replaces every emoji in <paramref name="text"/> with a fixed
    /// replacement string.
    /// </summary>
    /// <param name="text">The text to process, or <see langword="null"/>.</param>
    /// <param name="replacement">
    /// The string to substitute for each emoji match. <see langword="null"/>
    /// is treated as an empty string.
    /// </param>
    /// <returns>
    /// <paramref name="text"/> with every emoji replaced. Returns an empty
    /// string for <see langword="null"/> or empty input.
    /// </returns>
    public static string Replace(string? text, string? replacement)
    {
        var value = replacement ?? string.Empty;
        return ReplaceEach(text, _ => value);
    }

    /// <summary>
    /// Replaces every emoji in <paramref name="text"/> with the string
    /// produced by <paramref name="replacementSelector"/> for that match.
    /// </summary>
    /// <param name="text">The text to process, or <see langword="null"/>.</param>
    /// <param name="replacementSelector">
    /// A function that maps a matched emoji to its replacement string.
    /// </param>
    /// <returns>
    /// <paramref name="text"/> with every emoji replaced. Returns an empty
    /// string for <see langword="null"/> or empty input.
    /// </returns>
    public static string ReplaceEach(string? text, Func<EmojiMatch, string> replacementSelector)
    {
        ArgumentNullException.ThrowIfNull(replacementSelector);
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(text.Length);
        var previousEnd = 0;
        foreach (var match in EnumerateMatches(text))
        {
            builder.Append(text, previousEnd, match.Index - previousEnd);
            builder.Append(replacementSelector(match));
            previousEnd = match.Index + match.Length;
        }

        builder.Append(text, previousEnd, text.Length - previousEnd);
        return builder.ToString();
    }

    /// <summary>
    /// Collapses only the whitespace runs in <paramref name="value"/> that
    /// touch a recorded emoji removal site. A touched run becomes a single
    /// space, or is removed entirely if that would leave it at the very
    /// start or end of the result. Whitespace runs that never bordered a
    /// removal are copied through unchanged.
    /// </summary>
    private static string CollapseWhitespaceAroundRemovals(string value, IReadOnlyList<int> removalSites)
    {
        if (removalSites.Count == 0)
        {
            return value;
        }

        var builder = new StringBuilder(value.Length);
        var siteIndex = 0;
        var position = 0;
        while (position < value.Length)
        {
            var runStart = position;
            var isWhitespaceRun = char.IsWhiteSpace(value[position]);
            while (position < value.Length && char.IsWhiteSpace(value[position]) == isWhitespaceRun)
            {
                position++;
            }

            if (!isWhitespaceRun)
            {
                builder.Append(value, runStart, position - runStart);
                continue;
            }

            while (siteIndex < removalSites.Count && removalSites[siteIndex] < runStart)
            {
                siteIndex++;
            }

            var touchesRemoval = siteIndex < removalSites.Count && removalSites[siteIndex] <= position;
            if (!touchesRemoval)
            {
                builder.Append(value, runStart, position - runStart);
            }
            else if (runStart != 0 && position != value.Length)
            {
                builder.Append(WhitespaceCollapseChar);
            }
        }

        return builder.ToString();
    }
}
