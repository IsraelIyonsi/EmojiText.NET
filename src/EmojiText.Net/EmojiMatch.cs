namespace EmojiText;

/// <summary>
/// A single emoji match found in a string, reported as the user-perceived
/// unit it represents (which may span several UTF-16 chars for a ZWJ
/// sequence, flag, keycap, or skin-tone modified emoji).
/// </summary>
public readonly struct EmojiMatch : IEquatable<EmojiMatch>
{
    /// <summary>
    /// Initializes a new <see cref="EmojiMatch"/>.
    /// </summary>
    /// <param name="value">The matched emoji substring.</param>
    /// <param name="index">The UTF-16 char index where the match starts.</param>
    /// <param name="length">The UTF-16 char length of the match.</param>
    public EmojiMatch(string value, int index, int length)
    {
        Value = value;
        Index = index;
        Length = length;
    }

    /// <summary>
    /// Gets the matched emoji substring, exactly as it appears in the source
    /// text.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets the UTF-16 char index in the source text where the match starts.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Gets the length of the match in UTF-16 chars. This can be greater
    /// than 1 for a single user-perceived emoji, since ZWJ sequences, flags,
    /// keycaps, and skin-tone modifiers are all multi-codepoint.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Determines whether two matches represent the same value at the same
    /// position.
    /// </summary>
    public static bool operator ==(EmojiMatch left, EmojiMatch right) => left.Equals(right);

    /// <summary>
    /// Determines whether two matches differ in value or position.
    /// </summary>
    public static bool operator !=(EmojiMatch left, EmojiMatch right) => !left.Equals(right);

    /// <inheritdoc />
    public bool Equals(EmojiMatch other) =>
        Index == other.Index &&
        Length == other.Length &&
        string.Equals(Value, other.Value, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is EmojiMatch other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Value, Index, Length);

    /// <inheritdoc />
    public override string ToString() => Value;
}
