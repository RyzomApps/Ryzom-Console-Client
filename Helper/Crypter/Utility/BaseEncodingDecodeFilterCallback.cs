namespace RCC.Helper.Crypter.Utility
{
    /// <summary>
    /// A callback to map arbitrary characters onto the characters that can be decoded.
    /// </summary>
    /// <param name="originalCharacter">The original character.</param>
    /// <returns>the replacement character.</returns>
    public delegate char BaseEncodingDecodeFilterCallback(char originalCharacter);
}