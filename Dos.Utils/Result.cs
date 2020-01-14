using System;

namespace Dos.Utils
{
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFail => !IsSuccess;
        public string Message { get; }

        public Result(bool isSuccess, string message = null)
        {
            IsSuccess = isSuccess;
            Message = message;
        }

        public bool HasMessage => !string.IsNullOrEmpty(Message);

        public static Result Fail(string message = null) => new Result(false, message);
        public static Result Success(string message = null) => new Result(true, message);

        public Result IfSuccess(Func<Result, Result> successAction) => IsSuccess ? successAction(this) : this;

        public Result IfFail(Func<Result, Result> failActon) => IsFail ? failActon(this) : this;

        public override string ToString() =>
            $"({(IsSuccess ? "Success" : "Fail")}) - \"{Message}\"";
    }

    public class Result<T> : Result
    {
        public static readonly Result<T> DefaultFail = new Result<T>(false);
        public static readonly Result<T> DefaultSuccess = new Result<T>(true);

        public Result(bool isSuccess, T value = default(T), string message = null) : base(isSuccess, message)
        {
            Value = value;
        }

        public T Value { get; }

        public Result<TNext> Then<TNext>(Func<Result<T>, Result<TNext>> successAction,
                                         Func<Result<T>, Result<TNext>> failActon) =>
            IsSuccess ? successAction(this) : failActon(this);

        public new static Result<T> Fail(string message = null) => new Result<T>(false, message: message);
        public new static Result<T> Success(string message = null) => new Result<T>(true, message: message);

        public Result IfSuccess(Func<Result<T>, Result> successAction) => IsSuccess ? successAction(this) : this;

        public Result IfFail(Func<Result<T>, Result> failActon) => IsFail ? failActon(this) : this;

        public override string ToString() =>
            $"({(IsSuccess ? "Success" : "Fail")}) {Value}{(string.IsNullOrWhiteSpace(Message) ? string.Empty : $" - \"{Message}\"")}";
    }
}
