namespace RCC.Messages
{
    /// <summary>
    ///     A message field - TMessageFormat with message type and bit size
    /// </summary>
    internal class MessageField
    {
        byte _bitSize;
        FieldType _type;

        public MessageField(FieldType type, byte bitSize = 0)
        {
            _type = type;
            _bitSize = bitSize;
        }
    };
}