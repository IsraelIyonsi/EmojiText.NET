using EmojiText;

namespace EmojiText.Net.Tests.Api;

public class EmojiApiTests
{
    private const string ManSkinToneTone3ThumbsUp = "\U0001F44D\U0001F3FD";
    private const string GrinningFace = "\U0001F600";
    private const string ManBoyFamily = "\U0001F468‍\U0001F466";

    public static IEnumerable<object?[]> NullOrEmptyInputs()
    {
        yield return [null];
        yield return [""];
    }

    [Theory]
    [MemberData(nameof(NullOrEmptyInputs))]
    public void HasEmoji_returns_false_for_null_or_empty(string? text)
    {
        Assert.False(Emoji.HasEmoji(text));
    }

    [Theory]
    [MemberData(nameof(NullOrEmptyInputs))]
    public void Count_returns_zero_for_null_or_empty(string? text)
    {
        Assert.Equal(0, Emoji.Count(text));
    }

    [Theory]
    [MemberData(nameof(NullOrEmptyInputs))]
    public void EnumerateMatches_returns_empty_for_null_or_empty(string? text)
    {
        Assert.Empty(Emoji.EnumerateMatches(text));
    }

    [Theory]
    [MemberData(nameof(NullOrEmptyInputs))]
    public void Strip_returns_empty_string_for_null_or_empty(string? text)
    {
        Assert.Equal(string.Empty, Emoji.Strip(text));
    }

    [Theory]
    [MemberData(nameof(NullOrEmptyInputs))]
    public void Replace_returns_empty_string_for_null_or_empty(string? text)
    {
        Assert.Equal(string.Empty, Emoji.Replace(text, "X"));
    }

    [Theory]
    [InlineData("hello world", false)]
    [InlineData("hello world " + GrinningFace, true)]
    [InlineData(GrinningFace, true)]
    [InlineData("你好世界", false)]
    public void HasEmoji_detects_presence_correctly(string text, bool expected)
    {
        Assert.Equal(expected, Emoji.HasEmoji(text));
    }

    [Theory]
    [InlineData("no emoji here", 0)]
    [InlineData(GrinningFace, 1)]
    [InlineData(GrinningFace + GrinningFace, 2)]
    [InlineData("a" + GrinningFace + "b" + ManBoyFamily + "c", 2)]
    public void Count_counts_user_perceived_emoji_not_codepoints(string text, int expected)
    {
        Assert.Equal(expected, Emoji.Count(text));
    }

    [Fact]
    public void EnumerateMatches_reports_value_index_and_length_in_utf16_chars()
    {
        var text = "Hi " + ManBoyFamily + "!";
        var matches = Emoji.EnumerateMatches(text).ToArray();

        var match = Assert.Single(matches);
        Assert.Equal(ManBoyFamily, match.Value);
        Assert.Equal(3, match.Index);
        Assert.Equal(ManBoyFamily.Length, match.Length);
        Assert.Equal(5, match.Length);
    }

    [Fact]
    public void EnumerateMatches_is_lazy_and_reusable()
    {
        var text = GrinningFace + GrinningFace;
        var matches = Emoji.EnumerateMatches(text);

        Assert.Equal(2, matches.Count());
        Assert.Equal(2, matches.Count());
    }

    [Theory]
    [InlineData("Hello " + GrinningFace + " world", false, "Hello  world")]
    [InlineData("Hello " + GrinningFace + " world", true, "Hello world")]
    [InlineData(GrinningFace + " leading", true, "leading")]
    [InlineData("trailing " + GrinningFace, true, "trailing")]
    [InlineData("no emoji", true, "no emoji")]
    [InlineData("no emoji", false, "no emoji")]
    public void Strip_removes_emoji_and_optionally_collapses_whitespace(string text, bool collapse, string expected)
    {
        Assert.Equal(expected, Emoji.Strip(text, collapse));
    }

    [Fact]
    public void Strip_collapses_multiple_adjacent_emoji_tokens_to_single_space()
    {
        var text = "Rate " + ManSkinToneTone3ThumbsUp + " " + GrinningFace + " today";
        Assert.Equal("Rate today", Emoji.Strip(text, collapseWhitespace: true));
    }

    [Fact]
    public void Strip_with_collapseWhitespace_leaves_whitespace_untouched_when_it_never_bordered_an_emoji()
    {
        var text = "  Hello   " + GrinningFace + "   world  ";

        Assert.Equal("  Hello world  ", Emoji.Strip(text, collapseWhitespace: true));
    }

    [Fact]
    public void Strip_with_collapseWhitespace_does_not_collapse_whitespace_in_a_run_with_no_emoji_at_all()
    {
        var text = "no   emoji   here";

        Assert.Equal(text, Emoji.Strip(text, collapseWhitespace: true));
    }

    [Fact]
    public void Replace_substitutes_fixed_string_for_every_match()
    {
        var text = "Rate " + ManSkinToneTone3ThumbsUp + " today " + GrinningFace;
        Assert.Equal("Rate [emoji] today [emoji]", Emoji.Replace(text, "[emoji]"));
    }

    [Fact]
    public void Replace_treats_null_replacement_as_empty_string()
    {
        Assert.Equal("Rate  today", Emoji.Replace("Rate " + GrinningFace + " today", null));
    }

    [Fact]
    public void ReplaceEach_invokes_selector_once_per_match_with_correct_match_data()
    {
        var text = GrinningFace + "-" + ManBoyFamily;
        var seen = new List<EmojiMatch>();

        var result = Emoji.ReplaceEach(text, match =>
        {
            seen.Add(match);
            return $"<{match.Index}:{match.Length}>";
        });

        Assert.Equal("<0:2>-<3:5>", result);
        Assert.Equal(2, seen.Count);
        Assert.Equal(GrinningFace, seen[0].Value);
        Assert.Equal(ManBoyFamily, seen[1].Value);
    }

    [Fact]
    public void ReplaceEach_throws_for_null_selector()
    {
        Assert.Throws<ArgumentNullException>(() => Emoji.ReplaceEach("text", null!));
    }

    [Fact]
    public void ReplaceEach_can_replace_with_empty_string()
    {
        Assert.Equal("Hi !", Emoji.ReplaceEach("Hi " + GrinningFace + "!", _ => string.Empty));
    }

    [Fact]
    public void UnicodeEmojiVersion_is_exposed_and_pinned()
    {
        Assert.Equal("16.0", Emoji.UnicodeEmojiVersion);
    }
}
