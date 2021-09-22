#region License
/*
CryptSharp
Copyright (c) 2013 James F. Bellinger <http://www.zer7.com/software/cryptsharp>

Permission to use, copy, modify, and/or distribute this software for any
purpose with or without fee is hereby granted, provided that the above
copyright notice and this permission notice appear in all copies.

THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using RCC.Helper.Crypter.Internal;

namespace RCC.Helper.Crypter.Utility
{
    /// <summary>
    /// Performs generic binary-to-text encoding.
    /// </summary>
    public class BaseEncoding : Encoding
    {
        private readonly string _characters;
        private readonly Dictionary<char, int> _values;
        private readonly BaseEncodingDecodeFilterCallback _decodeFilterCallback;

        /// <summary>
        /// Defines a binary-to-text encoding.
        /// </summary>
        /// <param name="characterSet">The characters of the encoding.</param>
        /// <param name="msbComesFirst">
        ///     <c>true</c> to begin with the most-significant bit of each byte.
        ///     Otherwise, the encoding begins with the least-significant bit.
        /// </param>
        public BaseEncoding(string characterSet, bool msbComesFirst)
            : this(characterSet, msbComesFirst, null, null)
        {

        }

        /// <summary>
        /// Defines a binary-to-text encoding.
        /// Additional decode characters let you add aliases, and a filter callback can be used
        /// to make decoding case-insensitive among other things.
        /// </summary>
        /// <param name="characterSet">The characters of the encoding.</param>
        /// <param name="msbComesFirst">
        ///     <c>true</c> to begin with the most-significant bit of each byte.
        ///     Otherwise, the encoding begins with the least-significant bit.
        /// </param>
        /// <param name="additionalDecodeCharacters">
        ///     A dictionary of alias characters, or <c>null</c> if no aliases are desired.
        /// </param>
        /// <param name="decodeFilterCallback">
        ///     A callback to map arbitrary characters onto the characters that can be decoded.
        /// </param>
        public BaseEncoding(string characterSet, bool msbComesFirst,
                            IDictionary<char, int> additionalDecodeCharacters,
                            BaseEncodingDecodeFilterCallback decodeFilterCallback)
        {
            Check.Null("characterSet", characterSet);

            if (!BitMath.IsPositivePowerOf2(characterSet.Length))
            {
                throw Exceptions.Argument("characterSet",
                                          "Length must be a power of 2.");
            }

            if (characterSet.Length > 256)
            {
                throw Exceptions.Argument("characterSet",
                                          "Character sets with over 256 characters are not supported.");
            }

            BitsPerCharacter = 31 - BitMath.CountLeadingZeros(characterSet.Length);
            BitMask = (1 << BitsPerCharacter) - 1;
            _characters = characterSet;
            MsbComesFirst = msbComesFirst;
            _decodeFilterCallback = decodeFilterCallback;

            _values = additionalDecodeCharacters != null
                ? new Dictionary<char, int>(additionalDecodeCharacters)
                : new Dictionary<char, int>();
            for (int i = 0; i < characterSet.Length; i ++)
            {
                char ch = characterSet[i];
                if (_values.ContainsKey(ch))
                {
                    throw Exceptions.Argument("Duplicate characters are not supported.",
                                              "characterSet");
                }
                _values.Add(ch, (byte)i);
            }
        }

        /// <summary>
        /// Gets the value corresponding to the specified character.
        /// </summary>
        /// <param name="character">A character.</param>
        /// <returns>A value, or <c>-1</c> if the character is not part of the encoding.</returns>
        public virtual int GetValue(char character)
        {
            if (_decodeFilterCallback != null)
            {
                character = _decodeFilterCallback(character);
            }

            return _values.TryGetValue(character, out var value) ? value : -1;
        }

        /// <summary>
        /// Gets the character corresponding to the specified value.
        /// </summary>
        /// <param name="value">A value.</param>
        /// <returns>A character.</returns>
        public virtual char GetChar(int value)
        {
            return _characters[value & BitMask];
        }

        /// <summary>
        /// The bit mask for a single character in the current encoding.
        /// </summary>
        public int BitMask { get; }

        /// <summary>
        /// The number of bits per character in the current encoding.
        /// </summary>
        public int BitsPerCharacter { get; }

        /// <summary>
        /// <c>true</c> if the encoding begins with the most-significant bit of each byte.
        /// Otherwise, the encoding begins with the least-significant bit.
        /// </summary>
        public bool MsbComesFirst { get; }

        #region Decoding

        /// <inheritdoc />
        public override int GetMaxCharCount(int byteCount)
        {
            Check.Range("byteCount", byteCount, 0, int.MaxValue);

            return checked((byteCount * 8 + BitsPerCharacter - 1) / BitsPerCharacter);
        }

        /// <inheritdoc />
        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            int charCount = GetCharCount(bytes, byteIndex, byteCount);

            return GetChars(bytes, byteIndex, byteCount, chars, charIndex, charCount);
        }

        /// <summary>
        /// Converts bytes from their binary representation to a text representation.
        /// </summary>
        /// <param name="bytes">An input array of bytes.</param>
        /// <param name="byteIndex">The index of the first byte.</param>
        /// <param name="byteCount">The number of bytes to read.</param>
        /// <param name="chars">An output array of characters.</param>
        /// <param name="charIndex">The index of the first character.</param>
        /// <param name="charCount">The number of characters to write.</param>
        /// <returns>The number of characters written.</returns>
        public int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex, int charCount)
        {
            Check.Bounds("bytes", bytes, byteIndex, byteCount);
            Check.Bounds("chars", chars, charIndex, charCount);

            int byteEnd = checked(byteIndex + byteCount);

            int bitStartOffset = 0;
            for (int i = 0; i < charCount; i++)
            {
                byte value;

                byte thisByte = byteIndex + 0 < byteEnd ? bytes[byteIndex + 0] : (byte)0;
                byte nextByte = byteIndex + 1 < byteEnd ? bytes[byteIndex + 1] : (byte)0;

                int bitEndOffset = bitStartOffset + BitsPerCharacter;
                if (MsbComesFirst)
                {
                    value = BitMath.ShiftRight(thisByte, 8 - bitStartOffset - BitsPerCharacter);
                    if (bitEndOffset > 8)
                    {
                        value |= BitMath.ShiftRight(nextByte, 16 - bitStartOffset - BitsPerCharacter);
                    }
                }
                else
                {
                    value = BitMath.ShiftRight(thisByte, bitStartOffset);
                    if (bitEndOffset > 8)
                    {
                        value |= BitMath.ShiftRight(nextByte, bitStartOffset - 8);
                    }
                }

                bitStartOffset = bitEndOffset;
                if (bitStartOffset >= 8)
                {
                    bitStartOffset -= 8; byteIndex++;
                }

                chars[i] = GetChar(value);
            }

            return charCount;
        }

        /// <inheritdoc />
        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            Check.Bounds("bytes", bytes, index, count);

            return GetMaxCharCount(count);
        }

        #endregion

        #region Encoding

        /// <inheritdoc />
        public override int GetMaxByteCount(int charCount)
        {
            Check.Range("charCount", charCount, 0, int.MaxValue);

            return checked(charCount * BitsPerCharacter / 8);
        }

        /// <inheritdoc />
        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            int byteCount = GetByteCount(chars, charIndex, charCount);

            return GetBytes(chars, charIndex, charCount, bytes, byteIndex, byteCount);
        }

        /// <summary>
        /// Converts characters from their text representation to a binary representation.
        /// </summary>
        /// <param name="chars">An input array of characters.</param>
        /// <param name="charIndex">The index of the first character.</param>
        /// <param name="charCount">The number of characters to read.</param>
        /// <param name="bytes">An output array of bytes.</param>
        /// <param name="byteIndex">The index of the first byte.</param>
        /// <param name="byteCount">The number of bytes to write.</param>
        /// <returns>The number of bytes written.</returns>
        public int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex, int byteCount)
        {
            Check.Bounds("chars", chars, charIndex, charCount);
            Check.Bounds("bytes", bytes, byteIndex, byteCount);

            Array.Clear(bytes, byteIndex, byteCount);
            int byteEnd = checked(byteIndex + byteCount);

            int bitStartOffset = 0;
            for (int i = 0; i < charCount; i++)
            {
                byte value = (byte)GetValue(chars[i]);

                int bitEndOffset = bitStartOffset + BitsPerCharacter;
                if (MsbComesFirst)
                {
                    if (byteIndex < byteEnd)
                    {
                        bytes[byteIndex] |= BitMath.ShiftLeft(value, 8 - bitStartOffset - BitsPerCharacter);
                    }

                    if (byteIndex + 1 < byteEnd && bitEndOffset > 8)
                    {
                        bytes[byteIndex + 1] |= BitMath.ShiftLeft(value, 16 - bitStartOffset - BitsPerCharacter);
                    }
                }
                else
                {
                    if (byteIndex < byteEnd)
                    {
                        bytes[byteIndex] |= BitMath.ShiftLeft(value, bitStartOffset);
                    }

                    if (byteIndex + 1 < byteEnd && bitEndOffset > 8)
                    {
                        bytes[byteIndex + 1] |= BitMath.ShiftLeft(value, bitStartOffset - 8);
                    }
                }

                bitStartOffset = bitEndOffset;
                if (bitStartOffset >= 8)
                {
                    bitStartOffset -= 8; byteIndex++;
                }
            }

            return byteCount;
        }

        /// <inheritdoc />
        public override int GetByteCount(char[] chars, int index, int count)
        {
            Check.Bounds("chars", chars, index, count);

            return GetMaxByteCount(count);
        }

        #endregion
    }
}
