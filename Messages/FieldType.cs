namespace RCC.Messages
{
    /// <summary>
    ///     type of the variable used in MessageField of the MessageNode
    /// </summary>
    internal enum FieldType
    {
        Bool,
        Sint8,
        Sint16,
        Sint32,
        Sint64,
        Uint8,
        Uint16,
        Uint32,
        Uint64,
        BitSizedUint,
        Float,
        Double,
        EntityId,
        String,
        UcString
    };
}