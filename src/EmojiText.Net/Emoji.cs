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
    /// When <see langword="true"/>, every run of whitespace left in the
    /// result is collapsed to a single space and the result is trimmed of
    /// leading and trailing whitespace. When <see langword="false"/> (the
    /// default), whitespace that surrounded a removed emoji is left as-is,
    /// which commonly leaves doubled spaces behind.
    /// </param>
    /// <returns>
    /// <paramref name="text"/> with all emoji removed. Returns an empty
    /// string for <see langword="null"/> or empty input.
    /// </returns>
    public static string Strip(string? text, bool collapseWhitespace = false)
    {
        var stripped = Replace(text, string.Empty);
        return collapseWhitespace ? CollapseWhitespace(stripped) : stripped;
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

    private static string CollapseWhitespace(string value)
    {
        var builder = new StringBuilder(value.Length);
        var previousWasWhitespace = false;
        foreach (var ch in value)
        {
            if (char.IsWhiteSpace(ch))
            {
                if (!previousWasWhitespace)
                {
                    builder.Append(WhitespaceCollapseChar);
                }

                previousWasWhitespace = true;
            }
            else
            {
                builder.Append(ch);
                previousWasWhitespace = false;
            }
        }

        var start = 0;
        var end = builder.Length;
        while (start < end && builder[start] == WhitespaceCollapseChar)
        {
            start++;
        }

        while (end > start && builder[end - 1] == WhitespaceCollapseChar)
        {
            end--;
        }

        return builder.ToString(start, end - start);
    }
}
