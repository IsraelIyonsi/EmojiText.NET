using EmojiText;
using EmojiText.Net.Tests.Support;

namespace EmojiText.Net.Tests.Sequences;

/// <summary>
/// Hand-authored hotspot fixtures for the cases that are easy to get wrong:
/// ZWJ families and couples, subdivision flags, keycaps, adjacent country
/// flags, odd-length regional-indicator runs, skin-tone modifiers (including
/// a bare modifier matching standalone), string-boundary emoji, and text
/// that must never be mistaken for emoji.
/// </summary>
public class TrickySequenceFixtureTests
{
    private static readonly string FixturePath = Path.Combine(AppContext.BaseDirectory, "fixtures", "tricky-sequences.txt");

    public static IEnumerable<object[]> Cases() =>
        TrickySequenceCase.ReadAll(FixturePath).Select(c => new object[] { c });

    [Theory]
    [MemberData(nameof(Cases))]
    public void Tricky_case_produces_exactly_the_expected_matches(TrickySequenceCase testCase)
    {
        var actual = Emoji.EnumerateMatches(testCase.Text).ToArray();

        Assert.Equal(testCase.ExpectedMatches.Count, actual.Length);
        for (var i = 0; i < actual.Length; i++)
        {
            var (expectedIndex, expectedHex) = testCase.ExpectedMatches[i];
            var expectedValue = CodePointSequenceFixture.ToText(expectedHex);

            Assert.Equal(expectedIndex, actual[i].Index);
            Assert.Equal(expectedValue, actual[i].Value);
            Assert.Equal(expectedValue.Length, actual[i].Length);
        }

        Assert.Equal(testCase.ExpectedMatches.Count, Emoji.Count(testCase.Text));
        Assert.Equal(testCase.ExpectedMatches.Count > 0, Emoji.HasEmoji(testCase.Text));
    }

    [Fact]
    public void Fixture_has_the_expected_pinned_case_count()
    {
        Assert.Equal(23, TrickySequenceCase.ReadAll(FixturePath).Count());
    }
}
