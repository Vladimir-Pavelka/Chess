namespace ChessBoard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Board : IBoard
    {
        public readonly PieceType[,] BoardState = new PieceType[8, 8];
        public readonly bool IsWhitesTurn;
        public readonly Castling Castling;
        public readonly (int row, int col)? EnPassantTarget;
        public readonly int HalfmovesSinceLastCaptureOrPawnAdvance;
        public readonly int FullmoveCount = 1;

        public Board(string fen)
        {
            BoardState = FenNotation.GetBoardContent(fen);
            IsWhitesTurn = FenNotation.GetIsWhitesMove(fen);
            Castling = FenNotation.GetCastlingInfo(fen);
            EnPassantTarget = FenNotation.GetEnPassantTarget(fen);
            HalfmovesSinceLastCaptureOrPawnAdvance = FenNotation.GetHalfmovesSinceProgressCount(fen);
            FullmoveCount = FenNotation.GetFullmoveCount(fen);
        }

        private Board(Board previousState, Move move)
        {
            var newBoardState = (PieceType[,])previousState.BoardState.Clone();
            newBoardState[move.Source.row, move.Source.col] = PieceType.None;
            newBoardState[move.Destination.row, move.Destination.col] = move.PromotedInto ?? move.PieceType;

            if (move.MoveType == MoveType.EnPassantCapture)
            {
                var capturedPawnPos = (row: move.Source.row, col: move.Destination.col);
                newBoardState[capturedPawnPos.row, capturedPawnPos.col] = PieceType.None;
            }

            if (move.MoveType == MoveType.Castle)
            {
                var isKingsideCastle = move.Destination.col > move.Source.col;
                var row = move.Source.row;
                var rookSourcePos = isKingsideCastle ? (row: row, col: 7) : (row: row, col: 0);
                var rookDestPos = isKingsideCastle ? (row: row, col: 5) : (row: row, col: 3);
                newBoardState[rookDestPos.row, rookDestPos.col] = newBoardState[rookSourcePos.row, rookSourcePos.col];
                newBoardState[rookSourcePos.row, rookSourcePos.col] = PieceType.None;
            }

            BoardState = newBoardState;

            Castling = previousState.Castling;
            if (move.PieceType.HasFlag(PieceType.King)) Castling |= move.PieceType.HasFlag(PieceType.White) ? Castling.WhiteKingMoved : Castling.BlackKingMoved;
            if (move.PieceType.HasFlag(PieceType.Rook))
            {
                if (move.PieceType.HasFlag(PieceType.White)) Castling |= move.Source == (7, 7) ? Castling.WhiteKingsideRookMoved : Castling.WhiteQueensideRookMoved;
                else Castling |= move.Source == (0, 7) ? Castling.BlackKingsideRookMoved : Castling.BlackQueensideRookMoved;
            }

            IsWhitesTurn = !previousState.IsWhitesTurn;
            FullmoveCount = previousState.FullmoveCount + (IsWhitesTurn ? 1 : 0);
            HalfmovesSinceLastCaptureOrPawnAdvance = IsProgressMade(move) ? 0 : previousState.HalfmovesSinceLastCaptureOrPawnAdvance + 1;

            EnPassantTarget = move.MoveType == MoveType.EnPassantMove ? ((move.Source.row + move.Destination.row) / 2, move.Destination.col) : default((int, int)?);
        }

        private static bool IsProgressMade(Move move) =>
            move.MoveType == MoveType.Capture ||
            move.PieceType.HasFlag(PieceType.Pawn);

        public IBoard MakeMove(Move move) => new Board(this, move);

        public PieceType this[int row, int col] => BoardState[row, col];
        public PieceType this[(int row, int col) pos] => BoardState[pos.row, pos.col];
        public bool IsEmptyAt((int row, int col) pos) => BoardState[pos.row, pos.col] == PieceType.None;

        public IEnumerable<Piece> BoardPieces => BoardState.Select((type, pos) => new Piece(type, pos)).Where(p => p.Type != PieceType.None);
        public IEnumerable<Piece> WhitePieces => BoardPieces.Where(p => p.Type.HasFlag(PieceType.White));
        public IEnumerable<Piece> BlackPieces => BoardPieces.Where(p => !p.Type.HasFlag(PieceType.White));

        public IEnumerable<Move> WhiteMoves => throw new NotImplementedException();
        public IEnumerable<Move> BlackMoves => throw new NotImplementedException();

        public static Board NewGame()
        {
            var startingPositionFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
            return new Board(startingPositionFen);
        }

        public override string ToString() => FenNotation.GetFenString(this);
    }
}
