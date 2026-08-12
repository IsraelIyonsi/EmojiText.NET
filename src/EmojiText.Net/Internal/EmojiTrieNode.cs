namespace EmojiText.Internal;

/// <summary>
/// A single node in the <see cref="EmojiSequenceTrie"/>, keyed by Unicode
/// scalar value.
/// </summary>
internal sealed class EmojiTrieNode
{
    private Dictionary<int, EmojiTrieNode>? _children;

    /// <summary>
    /// Gets or sets a value indicating whether the path from the trie root to
    /// this node spells out a complete RGI emoji sequence.
    /// </summary>
    internal bool IsTerminal { get; set; }

    /// <summary>
    /// Gets the child node reached by <paramref name="codePoint"/>, creating
    /// it first if it does not already exist.
    /// </summary>
    /// <param name="codePoint">The Unicode scalar value to branch on.</param>
    internal EmojiTrieNode GetOrAddChild(int codePoint)
    {
        _children ??= [];
        if (!_children.TryGetValue(codePoint, out var child))
        {
            child = new EmojiTrieNode();
            _children[codePoint] = child;
        }

        return child;
    }

    /// <summary>
    /// Attempts to get the child node reached by <paramref name="codePoint"/>.
    /// </summary>
    /// <param name="codePoint">The Unicode scalar value to branch on.</param>
    /// <param name="child">The child node, when found.</param>
    /// <returns><see langword="true"/> if a matching child exists.</returns>
    internal bool TryGetChild(int codePoint, out EmojiTrieNode? child)
    {
        if (_children is null)
        {
            child = null;
            return false;
        }

        return _children.TryGetValue(codePoint, out child);
    }
}
