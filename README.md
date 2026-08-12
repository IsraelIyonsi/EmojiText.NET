# EmojiText.NET

Emoji-aware text processing for .NET: detect, count, strip, and replace emoji as the user-perceived units they are, not as raw code points. Zero external dependencies.

Emoji are not one `char`, and often not even one Unicode scalar value. A family is five code points joined by zero-width joiners. A flag is a pair of regional-indicator letters. A thumbs-up with a skin tone is a base emoji plus a modifier. Naive code that iterates `char` by `char`, or even scalar by scalar, sees fragments where a human sees one glyph, and any naive "strip the emoji" regex either misses these multi-codepoint sequences or shreds them into partial matches. `System.Globalization` does not expose an RGI emoji property either, so anything built on it inherits whatever the OS happens to know. EmojiText.NET ships its own pinned, embedded table of RGI emoji sequences derived directly from the official Unicode data files, so detection is exact and does not drift with the OS or the .NET runtime it happens to run on.

## Install

```
dotnet add package EmojiText.Net
```

## Usage

### Detect and count

```csharp
using EmojiText;

Emoji.HasEmoji("Nice work! \U0001F44D\U0001F3FD");  // true
Emoji.Count("Team \U0001F468‍\U0001F469‍\U0001F467‍\U0001F466 outing \U0001F600"); // 2
```

The family emoji above is five code points joined by zero-width joiners (`‍`), and the skin-toned thumbs-up is two code points (base + modifier). Both count as exactly one emoji each, because that is what a person sees.

### Strip emoji from user input before storage or logging

```csharp
using EmojiText;

string comment = "Loved it \U0001F600\U0001F389 will buy again!";
Emoji.Strip(comment);
// "Loved it  will buy again!"   (naive removal leaves the double space)

Emoji.Strip(comment, collapseWhitespace: true);
// "Loved it will buy again!"
```

### Enumerate matches for highlighting or redaction, or replace per match

```csharp
using EmojiText;

foreach (var match in Emoji.EnumerateMatches("Rate: \U0001F44D\U0001F3FD price: 5️⃣"))
{
    Console.WriteLine($"{match.Value} at {match.Index}, {match.Length} UTF-16 chars");
}

// Replace every emoji with a shortcode instead of deleting it
Emoji.ReplaceEach("So good \U0001F600", match => $":u{char.ConvertToUtf32(match.Value, 0):X4}:");
```

## API

A single static class, `Emoji`:

| Member | Purpose |
|---|---|
| `HasEmoji(string?)` | Whether the text contains any RGI emoji |
| `Count(string?)` | Number of emoji, each multi-codepoint sequence counted once |
| `EnumerateMatches(string?)` | Lazily yields `EmojiMatch { Value, Index, Length }` for every match, left to right, indices and lengths in UTF-16 chars |
| `Strip(string?, collapseWhitespace: false)` | Removes all emoji; optionally collapses and trims surrounding whitespace |
| `Replace(string?, string?)` | Replaces every emoji with a fixed string |
| `ReplaceEach(string?, Func<EmojiMatch, string>)` | Replaces every emoji with a string computed from that match |
| `UnicodeEmojiVersion` | The pinned Unicode emoji specification version the embedded data was built from |

All methods accept `null` and treat it the same as an empty string; no exceptions for the common case of an optional, absent field.

## What counts as an emoji here

Matching is scoped precisely to the `RGI_Emoji` property as defined by [UTS #51](https://unicode.org/reports/tr51/): the union of the `Basic_Emoji`, `Emoji_Keycap_Sequence`, `RGI_Emoji_Flag_Sequence`, `RGI_Emoji_Tag_Sequence`, and `RGI_Emoji_Modifier_Sequence` properties from `emoji-sequences.txt`, plus `RGI_Emoji_ZWJ_Sequence` from `emoji-zwj-sequences.txt`. Both files are published by the Unicode Consortium and this library embeds their fully-qualified sequences directly (Unicode emoji version 16.0, exposed as `Emoji.UnicodeEmojiVersion`).

Two consequences worth knowing:

- **"Unqualified" text-presentation symbols are excluded by design.** A bare `☺` (U+263A, no variation selector) is "unqualified" in Unicode's own test data and is not matched; `☺️` (U+263A U+FE0F) is fully-qualified and is matched. This is not an oversight, it is the RGI scope as Unicode defines it, and matches what modern keyboards and platforms actually emit.
- **Regional-indicator letters only match in the specific pairs Unicode recognizes as real or historical flags.** A lone regional indicator, or a pair that spells no assigned flag, is left alone rather than guessed at.

## Why this exists

.NET has no built-in `RGI_Emoji` Unicode property, and the popular emoji npm/Python packages have no direct, zero-dependency .NET equivalent that ships pinned, versioned Unicode data rather than reaching into OS globalization tables. Regex-based approaches built on a handwritten emoji range are what most teams reach for instead, and they reliably fail on exactly the cases that matter: ZWJ families, flags, and skin-tone modifiers get split into several fake "emoji," while newly assigned single code points are silently missed until someone remembers to update the pattern. EmojiText.NET replaces the regex with an exact match against the real Unicode data.

## Dependencies and AOT

Zero runtime NuGet dependencies. The library does no reflection over user types and performs no dynamic code generation; it reads its own embedded resource once via `Assembly.GetManifestResourceStream` and builds an in-memory trie. This is compatible with trimming and Native AOT in ordinary use. It has not been independently verified against the full Native AOT compatibility analyzer, so treat that as an expectation grounded in the implementation rather than a certified guarantee, and verify in your own AOT publish if that matters for your deployment.

## License

MIT. See [LICENSE](LICENSE).
