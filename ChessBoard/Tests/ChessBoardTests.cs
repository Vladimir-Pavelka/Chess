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

        [Fact]
        public void WhenNewGameIsInitialized_ThenFenStringShouldBeCalculatedCorrectly()
        {
            const string initialFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
            var board = Board.NewGame();

            var fen = board.ToString();

            fen.Should().Be(initialFen);
        }

        [Fact]
        public void WhenMovesArePerformed_ThenFenStringShouldBeCalculatedCorrectly()
        {
            var board = Board.NewGame();

            var move1 = new Move(PieceType.WhitePawn, MoveType.Move, Pos(e, 2), Pos(e, 4));
            var board2 = board.ExecuteMove(move1);
            board2.ToString().Should().Be("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1");

            var move2 = new Move(PieceType.BlackPawn, MoveType.Move, Pos(c, 7), Pos(c, 5));
            var board3 = board2.ExecuteMove(move2);
            board3.ToString().Should().Be("rnbqkbnr/pp1ppppp/8/2p5/4P3/8/PPPP1PPP/RNBQKBNR w KQkq c6 0 2");

            var move3 = new Move(PieceType.WhiteKnight, MoveType.Move, Pos(g, 1), Pos(f, 3));
            var board4 = board3.ExecuteMove(move3);
            board4.ToString().Should().Be("rnbqkbnr/pp1ppppp/8/2p5/4P3/5N2/PPPP1PPP/RNBQKB1R b KQkq - 1 2");
        }

        private static (int row, int col) Pos(int file, int rank) => (8 - rank, file - 1);

        private const int a = 1;
        private const int b = 2;
        private const int c = 3;
        private const int d = 4;
        private const int e = 5;
        private const int f = 6;
        private const int g = 7;
        private const int h = 8;
    }
}
