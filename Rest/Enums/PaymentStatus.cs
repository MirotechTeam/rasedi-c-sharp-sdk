using System.Runtime.Serialization;

namespace MiroPaySDK.Rest.Enums
{
    public enum PaymentStatuses
    {
        [EnumMember(Value = "TIMED_OUT")]
        TIMED_OUT,

        [EnumMember(Value = "PENDING")]
        PENDING,

        [EnumMember(Value = "PAID")]
        PAID,

        [EnumMember(Value = "CANCELED")]
        CANCELED,

        [EnumMember(Value = "FAILED")]
        FAILED,

        [EnumMember(Value = "SETTLED")]
        SETTLED
    }
}
