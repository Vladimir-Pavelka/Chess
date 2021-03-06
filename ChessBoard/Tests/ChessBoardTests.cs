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

            board[0, 0].Should().Be(PieceType.Rook);
            board[0, 7].Should().Be(PieceType.Rook);
            board[0, 1].Should().Be(PieceType.Knight);
            board[0, 6].Should().Be(PieceType.Knight);
            board[0, 2].Should().Be(PieceType.Bishop);
            board[0, 5].Should().Be(PieceType.Bishop);
            board[0, 3].Should().Be(PieceType.Queen);
            board[0, 4].Should().Be(PieceType.King);
            Enumerable.Range(0, 8).Select(i => board[1, i]).Should().AllBeEquivalentTo(PieceType.Pawn);

            board[7, 0].Should().Be(PieceType.Rook | PieceType.White);
            board[7, 7].Should().Be(PieceType.Rook | PieceType.White);
            board[7, 1].Should().Be(PieceType.Knight | PieceType.White);
            board[7, 6].Should().Be(PieceType.Knight | PieceType.White);
            board[7, 2].Should().Be(PieceType.Bishop | PieceType.White);
            board[7, 5].Should().Be(PieceType.Bishop | PieceType.White);
            board[7, 3].Should().Be(PieceType.Queen | PieceType.White);
            board[7, 4].Should().Be(PieceType.King | PieceType.White);
            Enumerable.Range(0, 8).Select(i => board[6, i]).Should().AllBeEquivalentTo(PieceType.Pawn | PieceType.White);

            Enumerable.Range(0, 8).Select(i => board[2, i]).Should().AllBeEquivalentTo(PieceType.None);
            Enumerable.Range(0, 8).Select(i => board[3, i]).Should().AllBeEquivalentTo(PieceType.None);
            Enumerable.Range(0, 8).Select(i => board[4, i]).Should().AllBeEquivalentTo(PieceType.None);
            Enumerable.Range(0, 8).Select(i => board[5, i]).Should().AllBeEquivalentTo(PieceType.None);
        }

        [Fact]
        public void WhenMovesArePerformed_ThenFenStringShouldBeCalculatedCorrectly()
        {
            var board = Board.NewGame();

            var move1 = new Move(PieceType.Pawn | PieceType.White, MoveType.EnPassantMove, Pos(e, 2), Pos(e, 4));
            var board2 = board.MakeMove(move1);
            board2.ToString().Should().Be("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1");

            var move2 = new Move(PieceType.Pawn, MoveType.EnPassantMove, Pos(c, 7), Pos(c, 5));
            var board3 = board2.MakeMove(move2);
            board3.ToString().Should().Be("rnbqkbnr/pp1ppppp/8/2p5/4P3/8/PPPP1PPP/RNBQKBNR w KQkq c6 0 2");

            var move3 = new Move(PieceType.Knight | PieceType.White, MoveType.Move, Pos(g, 1), Pos(f, 3));
            var board4 = board3.MakeMove(move3);
            board4.ToString().Should().Be("rnbqkbnr/pp1ppppp/8/2p5/4P3/5N2/PPPP1PPP/RNBQKB1R b KQkq - 1 2");
        }

        [Theory]
        [InlineData("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")]
        [InlineData("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1")]
        [InlineData("rnbqkbnr/pp1ppppp/8/2p5/4P3/8/PPPP1PPP/RNBQKBNR w KQkq c6 0 2")]
        [InlineData("rnbqkbnr/pp1ppppp/8/2p5/4P3/5N2/PPPP1PPP/RNBQKB1R b KQkq - 1 2")]
        public void WhenBoardInitializedFromFen_ThenToStringReturnsTheSameFen(string fen)
        {
            new Board(fen).ToString().Should().Be(fen);
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
