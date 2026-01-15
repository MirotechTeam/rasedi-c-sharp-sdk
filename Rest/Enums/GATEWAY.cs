using System.Runtime.Serialization;

namespace RasediSDK.Rest.Enums
{
    public enum GateWays
    {
        [EnumMember(Value = "ZAIN")]
        ZAIN,

        [EnumMember(Value = "FIB")]
        FIB,

        [EnumMember(Value = "ASIA_PAY")]
        ASIA_PAY,

        [EnumMember(Value = "FAST_PAY")]
        FAST_PAY,

        [EnumMember(Value = "NASS_WALLET")]
        NASS_WALLET,

        [EnumMember(Value = "CREDIT_CARD")]
        CREDIT_CARD,
    }
}
