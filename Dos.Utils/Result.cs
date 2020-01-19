using System;

namespace Dos.Utils
{
    public class Result
    {
        protected Result(bool isSuccess, string message = null)
        {
            IsSuccess = isSuccess;
            Message = message;
        }

        public bool IsSuccess { get; }
        public bool IsFail => !IsSuccess;
        public string Message { get; }

        public bool HasMessage => !string.IsNullOrEmpty(Message);

        public static Result Fail(string message = null) => new Result(false, message);
        public static Result Success(string message = null) => new Result(true, message);

        public Result IfSuccess(Func<Result, Result> successAction) => IsSuccess ? successAction(this) : this;

        public Result IfFail(Func<Result, Result> failActon) => IsFail ? failActon(this) : this;

        public Result DoIfSuccess(Action<Result> successAction)
        {
            if (IsSuccess)
                successAction(this);
            return this;
        }

        public Result DoIfFail(Action<Result> failAction)
        {
            if (IsFail)
                failAction(this);
            return this;
        }

        public Result ThenDo(Action<Result> successAction, Action<Result> failAction)
        {
            if (IsSuccess)
                successAction(this);
            else
                failAction(this);
            return this;
        }

        public override string ToString() =>
            $"({(IsSuccess ? "Success" : "Fail")}) - \"{Message}\"";
    }

    public class Result<T> : Result
    {
        public Result(bool isSuccess, T value = default, string message = null) : base(isSuccess, message)
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

        public Result<T> DoIfSuccess(Action<Result<T>> successAction)
        {
            if (IsSuccess)
                successAction(this);
            return this;
        }

        public Result<T> DoIfFail(Action<Result<T>> failAction)
        {
            if (IsFail)
                failAction(this);
            return this;
        }

        public Result<T> ThenDo(Action<Result<T>> successAction, Action<Result<T>> failAction)
        {
            if (IsSuccess)
                successAction(this);
            else
                failAction(this);
            return this;
        }

        public override string ToString() =>
            $"({(IsSuccess ? "Success" : "Fail")}) {Value}{(string.IsNullOrWhiteSpace(Message) ? string.Empty : $" - \"{Message}\"")}";
    }
}
