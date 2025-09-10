using System.Runtime.Serialization;

namespace MiroPaySDK.Rest.Enums
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

        [EnumMember(Value = "SUPER_QI")]
        SUPER_QI
    }
}
