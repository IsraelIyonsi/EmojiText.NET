namespace EmojiText.Internal;

/// <summary>
/// Scans text for RGI emoji matches using a greedy longest-match walk over
/// an <see cref="EmojiSequenceTrie"/>.
/// </summary>
internal static class EmojiScanner
{
    /// <summary>
    /// Enumerates every RGI emoji match in <paramref name="text"/>, in order,
    /// without overlap.
    /// </summary>
    /// <param name="text">The text to scan, or <see langword="null"/>.</param>
    /// <param name="trie">The RGI emoji sequence trie to match against.</param>
    internal static IEnumerable<EmojiMatch> Scan(string? text, EmojiSequenceTrie trie)
    {
        if (string.IsNullOrEmpty(text))
        {
            yield break;
        }

        var index = 0;
        while (index < text.Length)
        {
            var matchLength = trie.GetLongestMatchLength(text, index);
            if (matchLength > 0)
            {
                yield return new EmojiMatch(text.Substring(index, matchLength), index, matchLength);
                index += matchLength;
            }
            else
            {
                _ = CodePointReader.ReadAt(text, index, out var width);
                index += width;
            }
        }
    }
}
