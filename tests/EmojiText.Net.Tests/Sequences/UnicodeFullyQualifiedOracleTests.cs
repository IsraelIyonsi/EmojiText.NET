using EmojiText;
using EmojiText.Net.Tests.Support;

namespace EmojiText.Net.Tests.Sequences;

/// <summary>
/// Validates every fully-qualified RGI emoji sequence published in
/// unicode.org/Public/emoji/16.0/emoji-test.txt is recognized as exactly one
/// emoji spanning the whole sequence. This is the official Unicode reference
/// oracle for the library's core matching behavior.
/// </summary>
public class UnicodeFullyQualifiedOracleTests
{
    private static readonly string FixturePath = Path.Combine(AppContext.BaseDirectory, "fixtures", "fully-qualified-oracle.txt");

    public static IEnumerable<object[]> FullyQualifiedSequences() =>
        CodePointSequenceFixture.ReadSequenceLines(FixturePath).Select(hex => new object[] { hex });

    [Theory]
    [MemberData(nameof(FullyQualifiedSequences))]
    public void Fully_qualified_sequence_matches_as_exactly_one_emoji(string hexCodePoints)
    {
        var text = CodePointSequenceFixture.ToText(hexCodePoints);

        Assert.True(Emoji.HasEmoji(text));
        Assert.Equal(1, Emoji.Count(text));

        var match = Assert.Single(Emoji.EnumerateMatches(text));
        Assert.Equal(text, match.Value);
        Assert.Equal(0, match.Index);
        Assert.Equal(text.Length, match.Length);
    }

    [Theory]
    [MemberData(nameof(FullyQualifiedSequences))]
    public void Fully_qualified_sequence_embedded_in_sentence_matches_at_correct_offset(string hexCodePoints)
    {
        const string prefix = "Look: ";
        const string suffix = " done.";
        var emoji = CodePointSequenceFixture.ToText(hexCodePoints);
        var text = prefix + emoji + suffix;

        var match = Assert.Single(Emoji.EnumerateMatches(text));
        Assert.Equal(emoji, match.Value);
        Assert.Equal(prefix.Length, match.Index);
        Assert.Equal(emoji.Length, match.Length);
    }

    [Fact]
    public void Oracle_fixture_has_the_expected_pinned_row_count()
    {
        var count = CodePointSequenceFixture.ReadSequenceLines(FixturePath).Count();
        Assert.Equal(3781, count);
    }
}
