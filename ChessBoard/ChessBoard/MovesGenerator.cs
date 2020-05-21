namespace ChessBoard
{
    using System.Collections.Generic;

    class MovesGenerator
    {
        public IEnumerable<Move> GetAllAvailableMoves(PieceType[,] board, IDictionary<PieceType, (int row, int col)> pieces, CastlingInfo castlingInfo, Move lastMove)
        { 
            var isWhitesMove = !lastMove.IsWhitesMove;

            yield break;
        }

        private IEnumerable<Move> GetPawnMoves(KeyValuePair<PieceType, (int row, int col)> sourcePawn, PieceType[,] board, Move lastMove)
        {
            yield break;
        }
    }
}
