namespace Tests
{
    using ChessBoard;
    using FluentAssertions;
    using System.Linq;
    using Xunit;

    public class ChessBoardTests
    {
        [Fact]
        public void WhenNewGameIsInitialized_ThenPiecesShouldBeSetupCorrectly()
        {
            var board = Board.NewGame();

            board[0, 0].Should().Be(PieceType.BlackRook);
            board[0, 7].Should().Be(PieceType.BlackRook);
            board[0, 1].Should().Be(PieceType.BlackKnight);
            board[0, 6].Should().Be(PieceType.BlackKnight);
            board[0, 2].Should().Be(PieceType.BlackBishop);
            board[0, 5].Should().Be(PieceType.BlackBishop);
            board[0, 3].Should().Be(PieceType.BlackQueen);
            board[0, 4].Should().Be(PieceType.BlackKing);
            Enumerable.Range(0, 8).Select(i => board[1, i]).Should().AllBeEquivalentTo(PieceType.BlackPawn);

            board[7, 0].Should().Be(PieceType.WhiteRook);
            board[7, 7].Should().Be(PieceType.WhiteRook);
            board[7, 1].Should().Be(PieceType.WhiteKnight);
            board[7, 6].Should().Be(PieceType.WhiteKnight);
            board[7, 2].Should().Be(PieceType.WhiteBishop);
            board[7, 5].Should().Be(PieceType.WhiteBishop);
            board[7, 3].Should().Be(PieceType.WhiteQueen);
            board[7, 4].Should().Be(PieceType.WhiteKing);
            Enumerable.Range(0, 8).Select(i => board[6, i]).Should().AllBeEquivalentTo(PieceType.WhitePawn);

            Enumerable.Range(0, 8).Select(i => board[2, i]).Should().AllBeEquivalentTo(PieceType.None);
            Enumerable.Range(0, 8).Select(i => board[3, i]).Should().AllBeEquivalentTo(PieceType.None);
            Enumerable.Range(0, 8).Select(i => board[4, i]).Should().AllBeEquivalentTo(PieceType.None);
            Enumerable.Range(0, 8).Select(i => board[5, i]).Should().AllBeEquivalentTo(PieceType.None);
        }
    }
}
