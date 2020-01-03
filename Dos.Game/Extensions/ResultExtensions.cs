using Dos.Game.Util;

namespace Dos.Game.Extensions
{
    public static class ResultExtensions
    {
        public static Result<T> ToResult<T>(this T value, bool isSuccess = true) => new Result<T>(isSuccess, value);

        public static Result<T> ToSuccess<T>(this T value) => value.ToResult();

        public static Result<T> ToFail<T>(this T value) => value.ToResult(false);
    }
}
