namespace Tests
{
    using ChessBoard;
    using FluentAssertions;
    using System;
    using System.Collections.Generic;
    using Xunit;

    public class MovesGeneratorTests
    {
        [Theory]
        [MemberData(nameof(PositionsInbetweenTestData))]
        public void PositionsBetweenTwoFieldsAreGeneratedCorrectly((int, int) posA, (int, int) posB, (int, int)[] expected)
        {
            MovesGenerator.GetPositionsInbetween(posA, posB).Should().BeEquivalentTo(expected);
        }

        public static IEnumerable<object[]> PositionsInbetweenTestData => new[]
        {
            new object[]{ (1,1), (4,4), new[] { (2,2), (3,3) } },
            new object[]{ (4,4), (1,1), new[] { (2,2), (3,3) } },

            new object[]{ (1,4), (4,4), new[] { (2,4), (3,4) } },
            new object[]{ (4,4), (1,4), new[] { (2,4), (3,4) } },

            new object[]{ (2,5), (4,7), new[] { (3,6) } },
            new object[]{ (4,7), (2,5), new[] { (3,6) } },

            new object[]{ (1,1), (2,2), new (int, int)[] { } },
            new object[]{ (1,1), (1,2), new (int, int)[] { } }
        };
    }
}
