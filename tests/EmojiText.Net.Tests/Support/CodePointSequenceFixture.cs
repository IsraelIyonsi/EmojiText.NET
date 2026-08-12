using System.Globalization;
using System.Text;

namespace EmojiText.Net.Tests.Support;

/// <summary>
/// Reconstructs UTF-16 text from space-separated hex code point sequences,
/// mirroring the format used by the embedded production data file and the
/// test fixtures derived from it. Kept independent of the production code
/// under test so the oracle tests do not validate the library against its
/// own parsing logic.
/// </summary>
internal static class CodePointSequenceFixture
{
    private const char CodePointSeparator = ' ';
    private const char CommentPrefix = '#';

    internal static string ToText(string hexCodePoints)
    {
        var builder = new StringBuilder();
        foreach (var token in hexCodePoints.Split(CodePointSeparator))
        {
            var codePoint = int.Parse(token, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            builder.Append(char.ConvertFromUtf32(codePoint));
        }

        return builder.ToString();
    }

    internal static IEnumerable<string> ReadSequenceLines(string fixturePath)
    {
        foreach (var line in File.ReadLines(fixturePath))
        {
            if (line.Length == 0 || line[0] == CommentPrefix)
            {
                continue;
            }

            yield return line;
        }
    }
}
