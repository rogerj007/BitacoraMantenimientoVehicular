using System;

namespace BitacoraMatenimiento.Helpers
{
    public static class DateTimeExtensions
    {
        public static DateTime ToKindUtc(this DateTime value)
        {
            return KindUtc(value);
        }
        public static DateTime? ToKindUtc(this DateTime? value)
        {
            return KindUtc(value);
        }
        public static DateTime ToKindLocal(this DateTime value)
        {
            return KindLocal(value);
        }
        public static DateTime? ToKindLocal(this DateTime? value)
        {
            return KindLocal(value);
        }
        public static DateTime SpecifyKind(this DateTime value, DateTimeKind kind)
        {
            return value.Kind != kind ? DateTime.SpecifyKind(value, kind) : value;
        }
        public static DateTime? SpecifyKind(this DateTime? value, DateTimeKind kind)
        {
            return value.HasValue ? DateTime.SpecifyKind(value.Value, kind) : value;
        }
        public static DateTime KindUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => value
            };
        }
        public static DateTime? KindUtc(DateTime? value)
        {
            return value.HasValue ? KindUtc(value.Value) : value;
        }
        public static DateTime KindLocal(DateTime value)
        {
            if (value.Kind == DateTimeKind.Unspecified)
            {
                return DateTime.SpecifyKind(value, DateTimeKind.Local);
            }
            else if (value.Kind == DateTimeKind.Utc)
            {
                return value.ToLocalTime();
            }
            return value;
        }
        public static DateTime? KindLocal(DateTime? value)
        {
            if (value.HasValue)
            {
                return KindLocal(value.Value);
            }
            return value;
        }
    }
}
