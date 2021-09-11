namespace RCC.Msg
{
    /// <summary>
    /// A message field
    /// </summary>
    internal class CMessageField
    {
        TFieldType Type;
        byte BitSize;

        public CMessageField(TFieldType type, byte bitSize = 0)
        {
            Type = type;
            BitSize = bitSize;
        }
    };
}
