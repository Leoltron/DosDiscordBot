using System;
using FluentAssertions;
using NUnit.Framework;

namespace Dos.Utils.Tests
{
    public class AddShiftIndicesTests
    {
        [Test]
        public void NoShiftTest()
        {
            var baseArray = new[] {"0", "1", "2"};
            
            var expected = new[] {("0", 0), ("1", 1), ("2", 2)};
            baseArray.AddShiftIndices(0).Should().BeEquivalentTo(expected);
        }
        
        [Test]
        public void ShiftTest1()
        {
            var baseArray = new[] {"0", "1", "2"};
            
            var expected = new[] {("0", 2), ("1", 0), ("2", 1)};
            baseArray.AddShiftIndices(1).Should().BeEquivalentTo(expected);
        }
        
        [Test]
        public void ShiftTest2()
        {
            var baseArray = new[] {"0", "1", "2"};
            
            var expected = new[] {("0", 1), ("1", 2), ("2", 0)};
            baseArray.AddShiftIndices(2).Should().BeEquivalentTo(expected);
        }
        
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(-1)]
        public void ShiftTest3Exception(int index)
        {
            var baseArray = new[] {"0", "1", "2"};

            Action action = () => baseArray.AddShiftIndices(index);
            action.Should().Throw<IndexOutOfRangeException>();
        }
    }
}
