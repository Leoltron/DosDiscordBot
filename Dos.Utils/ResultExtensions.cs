using System.Threading.Tasks;

namespace Dos.Utils
{
    public static class ResultExtensions
    {
        public static Result<T> ToResult<T>(this T value, bool isSuccess, string message = null) =>
            new(isSuccess, value, message);

        public static Result<T> ToSuccess<T>(this T value, string message = null) => value.ToResult(true, message);

        public static Result<T> ToFail<T>(this T value, string message = null) => value.ToResult(false, message);

        public static Result<T> ToResult<T>(this Task<T> task) =>
            task.Status == TaskStatus.RanToCompletion ? task.Result.ToSuccess() : Result<T>.Fail();
    }
}
