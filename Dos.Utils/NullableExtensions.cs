using System;

namespace Dos.Utils
{
    public static class NullableExtensions
    {
        public static TResult IfHasValue<TValue, TResult>(this TValue? nullable,
                                                          Func<TValue, TResult> func,
                                                          TResult defaultValue = default) where TValue : struct =>
            nullable.HasValue ? func(nullable.Value) : defaultValue;

        public static TValue? DoIfHasValue<TValue>(this TValue? nullable, Action<TValue> action) where TValue : struct
        {
            if (nullable.HasValue)
                action(nullable.Value);

            return nullable;
        }
    }
}
