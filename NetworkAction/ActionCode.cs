namespace RCC.NetworkAction
{
    /// <summary>
    ///     type of the action or action property
    /// </summary>
    public enum ActionCode : byte
    {
        ActionPositionCode = 0,
        ActionGenericCode = 1,
        ActionGenericMultiPartCode = 2,
        ActionSint64 = 3,

        ActionSyncCode = 10,
        ActionDisconnectionCode = 11,
        ActionAssociationCode = 12,
        ActionLoginCode = 13,

        ActionTargetSlotCode = 40,

        ActionDummyCode = 99
    }
}