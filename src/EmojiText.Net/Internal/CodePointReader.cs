namespace EmojiText.Internal;

/// <summary>
/// Decodes Unicode scalar values from UTF-16 text one code point at a time,
/// treating surrogate pairs as a single scalar and reporting how many UTF-16
/// chars each scalar occupied.
/// </summary>
internal static class CodePointReader
{
    private const int SurrogatePairWidth = 2;
    private const int SingleCharWidth = 1;

    /// <summary>
    /// Reads the Unicode scalar value starting at <paramref name="index"/> in
    /// <paramref name="text"/>. A well-formed surrogate pair is combined into
    /// one scalar; an unpaired surrogate or any other char is returned as its
    /// own scalar value.
    /// </summary>
    /// <param name="text">The text to read from.</param>
    /// <param name="index">The UTF-16 char index to start reading at.</param>
    /// <param name="utf16Width">
    /// Set to 2 when a surrogate pair was consumed, otherwise 1.
    /// </param>
    /// <returns>The decoded Unicode scalar value.</returns>
    internal static int ReadAt(string text, int index, out int utf16Width)
    {
        var current = text[index];
        var nextIndex = index + 1;
        if (char.IsHighSurrogate(current) && nextIndex < text.Length && char.IsLowSurrogate(text[nextIndex]))
        {
            utf16Width = SurrogatePairWidth;
            return char.ConvertToUtf32(current, text[nextIndex]);
        }

        utf16Width = SingleCharWidth;
        return current;
    }
}
