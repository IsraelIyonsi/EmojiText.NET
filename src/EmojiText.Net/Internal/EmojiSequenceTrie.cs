namespace EmojiText.Internal;

/// <summary>
/// A prefix tree of RGI emoji code point sequences, used to find the longest
/// emoji match starting at a given position in a string. Longest-match
/// walking is what lets a ZWJ family or a flag pair be recognized as a single
/// emoji instead of being decomposed into its individual code points.
/// </summary>
internal sealed class EmojiSequenceTrie
{
    private readonly EmojiTrieNode _root = new();

    /// <summary>
    /// Initializes the trie from a set of RGI emoji code point sequences.
    /// </summary>
    /// <param name="sequences">
    /// The complete set of code point sequences to index, each already
    /// expanded to its full list of Unicode scalar values.
    /// </param>
    internal EmojiSequenceTrie(IEnumerable<int[]> sequences)
    {
        var count = 0;
        foreach (var sequence in sequences)
        {
            var node = _root;
            foreach (var codePoint in sequence)
            {
                node = node.GetOrAddChild(codePoint);
            }

            node.IsTerminal = true;
            count++;
        }

        SequenceCount = count;
    }

    /// <summary>
    /// Gets the number of distinct RGI emoji sequences indexed by this trie.
    /// </summary>
    internal int SequenceCount { get; }

    /// <summary>
    /// Finds the length, in UTF-16 chars, of the longest RGI emoji sequence
    /// that starts exactly at <paramref name="startIndex"/> in
    /// <paramref name="text"/>.
    /// </summary>
    /// <param name="text">The text to scan.</param>
    /// <param name="startIndex">The UTF-16 char index to start scanning at.</param>
    /// <returns>
    /// The UTF-16 char length of the longest match, or 0 when no RGI emoji
    /// sequence starts at <paramref name="startIndex"/>.
    /// </returns>
    internal int GetLongestMatchLength(string text, int startIndex)
    {
        var node = _root;
        var position = startIndex;
        var longestMatchEnd = -1;

        while (position < text.Length)
        {
            var codePoint = CodePointReader.ReadAt(text, position, out var width);
            if (!node.TryGetChild(codePoint, out var next))
            {
                break;
            }

            node = next!;
            position += width;
            if (node.IsTerminal)
            {
                longestMatchEnd = position;
            }
        }

        return longestMatchEnd < 0 ? 0 : longestMatchEnd - startIndex;
    }
}
