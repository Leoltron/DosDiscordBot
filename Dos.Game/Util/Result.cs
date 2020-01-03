using System;

namespace Dos.Game.Util
{
    public class Result<T>
    {
        public Result(bool isSuccess, T value = default(T))
        {
            IsSuccess = isSuccess;
            Value = value;
        }

        public bool IsSuccess { get; }
        public T Value { get; }

        public Result<T> IfSuccess(Func<T, Result<T>> successAction) => IsSuccess ? successAction(Value) : this;

        public Result<T> IfFail(Func<T, Result<T>> failActon) => IsSuccess ? this : failActon(Value);

        public Result<TNext> Then<TNext>(Func<T, Result<TNext>> successAction, Func<T, Result<TNext>> failActon) =>
            IsSuccess ? successAction(Value) : failActon(Value);
    }

    public static class ResultExtensions
    {
        public static Result<T> ToResult<T>(this T value, bool isSuccess = true) => new Result<T>(isSuccess, value);

        public static Result<T> ToSuccess<T>(this T value) => value.ToResult();

        public static Result<T> ToFail<T>(this T value) => value.ToResult(false);
    }
}
