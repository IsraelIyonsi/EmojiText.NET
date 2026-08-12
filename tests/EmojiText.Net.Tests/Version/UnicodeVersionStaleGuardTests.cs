using EmojiText;
using EmojiText.Internal;

namespace EmojiText.Net.Tests.Version;

/// <summary>
/// Guards against the embedded emoji data drifting silently out of sync with
/// the pinned <see cref="Emoji.UnicodeEmojiVersion"/> constant. If the data
/// file is ever regenerated from a newer Unicode emoji release without also
/// bumping the constant (or vice versa), these tests fail loudly.
/// </summary>
public class UnicodeVersionStaleGuardTests
{
    private const int PinnedSequenceCount = 3790;

    [Fact]
    public void Public_version_constant_matches_embedded_data_header()
    {
        Assert.Equal(Emoji.UnicodeEmojiVersion, EmojiDataProvider.DataVersion);
    }

    [Fact]
    public void Embedded_trie_contains_the_pinned_sequence_count()
    {
        Assert.Equal(PinnedSequenceCount, EmojiDataProvider.Trie.SequenceCount);
    }
}
