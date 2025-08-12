using System.Runtime.Serialization;

namespace MiroPaySDK.Rest.Enums
{
    public enum GateWays
    {
        [EnumMember(Value = "ZAIN")]
        ZAIN,

        [EnumMember(Value = "FIB")]
        FIB
    }
}
