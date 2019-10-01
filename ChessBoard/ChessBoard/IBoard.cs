using System.Collections.Generic;

namespace ChessBoard
{
    public interface IBoard
    {
        IEnumerable<Move> WhiteMoves { get; }
        IEnumerable<Move> BlackMoves { get; }
        IBoard ExecuteMove(Move move);
    }
}
