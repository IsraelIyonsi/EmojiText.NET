namespace EmojiText.Internal;

/// <summary>
/// Scans text for RGI emoji matches using a greedy longest-match walk over
/// an <see cref="EmojiSequenceTrie"/>.
/// </summary>
internal static class EmojiScanner
{
    /// <summary>
    /// The first and last Unicode scalar values of the regional indicator
    /// symbol block (see <see href="https://unicode.org/reports/tr51/">UTS #51</see>).
    /// A flag is exactly two of these, and per UAX #29 (grapheme cluster
    /// boundary rules GB12/GB13) a run of regional indicators is always
    /// partitioned into pairs starting from the first one in the run,
    /// regardless of whether any given pair spells an RGI-assigned flag.
    /// </summary>
    private const int RegionalIndicatorStart = 0x1F1E6;
    private const int RegionalIndicatorEnd = 0x1F1FF;

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
            var codePoint = CodePointReader.ReadAt(text, index, out var width);
            if (IsRegionalIndicator(codePoint))
            {
                index += ConsumeRegionalIndicatorPair(text, index, width, trie, out var flagMatch);
                if (flagMatch is not null)
                {
                    yield return flagMatch.Value;
                }

                continue;
            }

            var matchLength = trie.GetLongestMatchLength(text, index);
            if (matchLength > 0)
            {
                yield return new EmojiMatch(text.Substring(index, matchLength), index, matchLength);
                index += matchLength;
            }
            else
            {
                index += width;
            }
        }
    }

    /// <summary>
    /// Consumes a single regional-indicator pairing step starting at
    /// <paramref name="index"/>, enforcing UAX #29's strict left-to-right
    /// pairing so that a regional indicator already claimed by the pair
    /// ending here can never be re-paired with a later one. This is what a
    /// grapheme-cluster-aware renderer actually shows: an odd regional
    /// indicator out never borrows its neighbor's partner.
    /// </summary>
    /// <param name="text">The text being scanned.</param>
    /// <param name="index">The UTF-16 char index of the first regional indicator.</param>
    /// <param name="firstWidth">The UTF-16 width of the regional indicator at <paramref name="index"/>.</param>
    /// <param name="trie">The RGI emoji sequence trie to match against.</param>
    /// <param name="flagMatch">
    /// The flag match when the pair is an RGI-recognized flag sequence;
    /// otherwise <see langword="null"/>, including when the run has no
    /// second regional indicator to pair with.
    /// </param>
    /// <returns>
    /// The number of UTF-16 chars consumed: both regional indicators when a
    /// pair was formed (whether or not it is an RGI flag), otherwise just
    /// the lone regional indicator at <paramref name="index"/>.
    /// </returns>
    private static int ConsumeRegionalIndicatorPair(string text, int index, int firstWidth, EmojiSequenceTrie trie, out EmojiMatch? flagMatch)
    {
        var partnerIndex = index + firstWidth;
        if (partnerIndex < text.Length)
        {
            var partnerCodePoint = CodePointReader.ReadAt(text, partnerIndex, out var partnerWidth);
            if (IsRegionalIndicator(partnerCodePoint))
            {
                var pairLength = firstWidth + partnerWidth;
                var matchLength = trie.GetLongestMatchLength(text, index);
                flagMatch = matchLength == pairLength
                    ? new EmojiMatch(text.Substring(index, pairLength), index, pairLength)
                    : null;
                return pairLength;
            }
        }

        flagMatch = null;
        return firstWidth;
    }

    private static bool IsRegionalIndicator(int codePoint) =>
        codePoint is >= RegionalIndicatorStart and <= RegionalIndicatorEnd;
}
