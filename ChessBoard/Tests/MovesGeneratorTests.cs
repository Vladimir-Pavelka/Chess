namespace Tests
{
    using ChessBoard;
    using FluentAssertions;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class MovesGeneratorTests
    {
        private readonly MovesGenerator generator = new MovesGenerator();

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

        [Theory]
        [MemberData(nameof(IsFieldAttackedTestData))]
        public void IsFieldAttackedWorksCorrectly((int, int) pos, bool isDefenderWhite, bool isAttacked)
        {
            var board = Board.NewGame();
            MovesGenerator.IsAttacked(pos, isDefenderWhite, board).Should().Be(isAttacked);
        }

        public static IEnumerable<object[]> IsFieldAttackedTestData =>
            Enumerable.Range(0, 8).Select(col => new object[] { (6, col), false, true }) // all white pawns are initially defended
            .Concat(new[] { new object[] { (7, 0), false, false }, new object[] { (7, 7), false, false } }) // rooks are not
            .Concat(new[] { new object[] { (5, 1), false, true } }) // pawn attack works
            .Concat(Enumerable.Range(0, 8).Select(col => new object[] { (4, col), false, false })) // 4th rank is not attacked by white
            .Concat(Enumerable.Range(0, 8).Select(col => new object[] { (3, col), true, false })) // 5th rank is not attacked by black
            .ToArray();

        [Fact]
        public void GivenBoardWithBothCastleAvailable_ShouldReturnTwoCastlingMoves()
        {
            var board = new Board("8/8/8/b1b1b1b1/PbPbPbPb/RPRPRPRP/PPPPPPPP/R3K2R w KQ - 0 1");

            var moves = generator.GetAllMoves(board);

            moves.Should().HaveCount(9);
            moves.Where(m => m.MoveType == MoveType.Castle).Should().HaveCount(2);
            moves.Where(m => m.PieceType.HasFlag(PieceType.King) && m.MoveType != MoveType.Castle).Should().HaveCount(2);
            moves.Where(m => m.PieceType.HasFlag(PieceType.Rook)).Should().HaveCount(5);
        }

        [Fact]
        public void GivenBoardWithCastlingUnavailableDueToBlockingCheck_ShouldNotReturnCastlingMoves()
        {
            var board = new Board("8/8/8/1b1b1b1b/bPbPbPbP/PRPRPRPR/PPnPPPPP/R3K2R w KQ - 0 1");

            var moves = generator.GetAllMoves(board);

            moves.Should().HaveCount(7);
            moves.Where(m => m.MoveType == MoveType.Castle).Should().HaveCount(0);
            moves.Where(m => m.PieceType.HasFlag(PieceType.King) && m.MoveType != MoveType.Castle).Should().HaveCount(2);
            moves.Where(m => m.PieceType.HasFlag(PieceType.Rook)).Should().HaveCount(5);
        }

        [Fact]
        public void GivenBoardWithPawnInStartingPosition_ShouldReturnSingleAndDoubleSteps()
        {
            var board = new Board("8/8/8/8/8/8/P7/8 w - - 0 1");

            var moves = generator.GetAllMoves(board);

            moves.Should().HaveCount(2);
            moves.Where(m => m.MoveType == MoveType.Move).Should().HaveCount(1);
            moves.Where(m => m.MoveType == MoveType.EnPassantMove).Should().HaveCount(1);
        }

        [Fact]
        public void GivenPawnWithRegularMovePromoteAndCapturePromote_ShouldReturnTwoTimesFourPromoteOptions()
        {
            var board = new Board("3r4/4P3/8/8/8/8/8/8 w - - 0 1");

            var moves = generator.GetAllMoves(board);

            moves.Should().HaveCount(8);
            moves.Where(m => m.MoveType == MoveType.Capture).Should().HaveCount(4).And.Match(x => x.All(_ => _.PromotedInto.HasValue));
            moves.Where(m => m.MoveType == MoveType.Move).Should().HaveCount(4).And.Match(x => x.All(_ => _.PromotedInto.HasValue));
        }

        [Fact]
        public void KnightIsBlockedByAlly()
        {
            var board = new Board("8/8/8/8/8/2p5/2P5/N7 w - - 0 1");

            var moves = generator.GetAllMoves(board);

            moves.Should().HaveCount(1).And.Match(m => m.All(_ => _.PieceType.HasFlag(PieceType.Knight) && _.MoveType == MoveType.Move));
        }
    }
}
