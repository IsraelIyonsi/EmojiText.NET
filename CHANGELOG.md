# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2026-08-12

### Added

- `Emoji` static API: `HasEmoji`, `Count`, `EnumerateMatches`, `Strip` (with optional whitespace collapsing), `Replace`, and `ReplaceEach`, all operating on emoji as user-perceived units rather than raw code points.
- `EmojiMatch` readonly struct reporting `Value`, `Index`, and `Length`, with index and length always in UTF-16 chars.
- Embedded, pinned RGI emoji sequence table derived directly from the official Unicode `emoji-sequences.txt` and `emoji-zwj-sequences.txt` data files (Unicode emoji version 16.0), exposed as `Emoji.UnicodeEmojiVersion`. Detection does not depend on runtime globalization data.
- Correct handling of ZWJ sequences (families, couples, professions), regional-indicator flag pairs, subdivision flag tag sequences, keycap sequences, and skin-tone/hair modifier sequences as single matches, via a greedy longest-match trie walk over decoded Unicode scalar values.
- Surrogate-pair-safe scanning: astral code points are read as one scalar, and unpaired surrogates are handled without throwing or producing false matches.
- Verified against the official Unicode `emoji-test.txt` fully-qualified reference vectors (3,781 sequences), each asserted to match as exactly one emoji spanning its full text.
- Hand-authored fixtures for known hotspots: couple-with-heart and family ZWJ sequences, subdivision flags, keycaps, adjacent country flags, skin-tone modifiers, and emoji at string boundaries.
- A stale-guard test pinning both the exposed Unicode emoji version and the total embedded sequence count, so the data file and the version constant cannot silently drift apart.
- Zero runtime dependencies.
