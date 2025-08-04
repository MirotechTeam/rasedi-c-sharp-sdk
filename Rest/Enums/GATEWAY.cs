using System.Runtime.Serialization;

namespace MiroPaySDK.Rest.Enums
{
    public enum GATEWAY
    {
        [EnumMember(Value = "ZAIN")]
        ZAIN,

        [EnumMember(Value = "FIB")]
        FIB
    }
}
