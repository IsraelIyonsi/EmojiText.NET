using EmojiText;

namespace EmojiText.Net.Tests.Negative;

public class NegativeAndEdgeCaseTests
{
    public static IEnumerable<object[]> NonEmojiTexts()
    {
        yield return ["plain ascii sentence with punctuation! @#$%^&*()"];
        yield return ["digits 0123456789 and symbols +=-_[]{}"];
        yield return ["你好，世界"];
        yield return ["Привет, мир"];
        yield return ["́combining acute accent alone"];
        yield return ["control\tchars\nand\rnewlines"];
        yield return ["\0embedded null char"];
        yield return [new string('a', 10_000)];
    }

    [Theory]
    [MemberData(nameof(NonEmojiTexts))]
    public void Text_without_emoji_never_falsely_matches(string text)
    {
        Assert.False(Emoji.HasEmoji(text));
        Assert.Equal(0, Emoji.Count(text));
        Assert.Empty(Emoji.EnumerateMatches(text));
        Assert.Equal(text, Emoji.Strip(text));
        Assert.Equal(text, Emoji.Replace(text, "X"));
    }

    [Fact]
    public void Unpaired_high_surrogate_does_not_throw_and_is_not_matched()
    {
        var text = "before" + '\uD800' + "after";

        var matches = Emoji.EnumerateMatches(text).ToArray();

        Assert.Empty(matches);
        Assert.False(Emoji.HasEmoji(text));
        Assert.Equal(text, Emoji.Strip(text));
    }

    [Fact]
    public void Unpaired_low_surrogate_does_not_throw_and_is_not_matched()
    {
        var text = "before" + '\uDC00' + "after";

        var matches = Emoji.EnumerateMatches(text).ToArray();

        Assert.Empty(matches);
        Assert.False(Emoji.HasEmoji(text));
    }

    [Fact]
    public void Text_presentation_symbol_without_variation_selector_is_not_rgi_emoji()
    {
        // U+263A alone is "unqualified" per Unicode; only "263A FE0F" is the
        // fully-qualified RGI Basic_Emoji sequence.
        var text = "smiley ☺ here";

        Assert.False(Emoji.HasEmoji(text));
    }

    [Fact]
    public void Text_presentation_symbol_with_variation_selector_is_rgi_emoji()
    {
        var text = "smiley ☺️ here";

        Assert.True(Emoji.HasEmoji(text));
        Assert.Equal(1, Emoji.Count(text));
    }

    [Fact]
    public void Isolated_regional_indicator_is_not_matched()
    {
        var text = "flag letter " + char.ConvertFromUtf32(0x1F1F3) + " alone";

        Assert.False(Emoji.HasEmoji(text));
    }

    [Fact]
    public void Bare_digit_without_keycap_combining_marks_is_not_matched()
    {
        Assert.False(Emoji.HasEmoji("room 3 only"));
    }

    [Fact]
    public void Zwj_sequence_is_not_decomposed_into_its_base_emoji_and_a_literal_zwj_match()
    {
        var family = "\U0001F468‍\U0001F466";
        var matches = Emoji.EnumerateMatches(family).ToArray();

        var match = Assert.Single(matches);
        Assert.Equal(family, match.Value);
    }

    [Fact]
    public void Large_input_with_many_emoji_is_processed_without_error()
    {
        var singleEmoji = "\U0001F600";
        var text = string.Concat(Enumerable.Repeat(singleEmoji + " ", 5_000));

        Assert.Equal(5_000, Emoji.Count(text));
        Assert.True(Emoji.HasEmoji(text));

        var stripped = Emoji.Strip(text, collapseWhitespace: true);
        Assert.Equal(string.Empty, stripped);
    }

    [Fact]
    public void ReplaceEach_selector_receiving_original_match_can_round_trip_value()
    {
        var text = "before \U0001F600 after";
        var roundTripped = Emoji.ReplaceEach(text, match => match.Value);

        Assert.Equal(text, roundTripped);
    }

    [Fact]
    public void Consecutive_calls_are_deterministic()
    {
        var text = "a \U0001F600 b \U0001F44D\U0001F3FD c";

        var first = Emoji.EnumerateMatches(text).ToArray();
        var second = Emoji.EnumerateMatches(text).ToArray();

        Assert.Equal(first.Select(m => (m.Index, m.Length, m.Value)), second.Select(m => (m.Index, m.Length, m.Value)));
    }
}
