namespace RCC
{
    public enum TActionCode
    {
        ACTION_POSITION_CODE = 0,
        ACTION_GENERIC_CODE = 1,
        ACTION_GENERIC_MULTI_PART_CODE = 2,
        ACTION_SINT64 = 3,

        ACTION_SYNC_CODE = 10,
        ACTION_DISCONNECTION_CODE = 11,
        ACTION_ASSOCIATION_CODE = 12,
        ACTION_LOGIN_CODE = 13,

        ACTION_TARGET_SLOT_CODE = 40,

        ACTION_DUMMY_CODE = 99,
    }
}