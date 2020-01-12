using Dos.Utils;
using FluentAssertions;

namespace Dos.Game.Tests.Util
{
    public static class ResultExtensions
    {
        public static void ShouldBeFail(this Result result) => result.IsFail.Should().BeTrue();
        public static void ShouldBeSuccess(this Result result) => result.IsSuccess.Should().BeTrue();
    }
}
