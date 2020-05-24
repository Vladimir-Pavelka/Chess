namespace ChessBoard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Board : IBoard
    {
        private readonly PieceType[,] _boardState = new PieceType[8, 8];
        public Castling Castling { get; }

        private readonly int _fullmoveCount = 1;
        private readonly int _halfmovesSinceLastCaptureOrPawnAdvance = 0;

        public Board(string fen)
        {
            _boardState = FenNotation.GetBoardContent(fen);
            Castling = FenNotation.GetCastlingInfo(fen);

            IsWhitesTurn = FenNotation.GetIsWhitesMove(fen);
            _fullmoveCount = FenNotation.GetFullmoveCount(fen);
            _halfmovesSinceLastCaptureOrPawnAdvance = FenNotation.GetHalfmovesSinceProgressCount(fen);

            LastMove = FenNotation.GetEnPassantMove(fen);

            if (LastMove.MoveType == MoveType.EnPassantMove)
                EnPassantTarget = ((LastMove.Source.row + LastMove.Destination.row) / 2, LastMove.Destination.col);
        }

        private Board(Board previousState, Move move)
        {
            var newBoardState = (PieceType[,])previousState._boardState.Clone();
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

            _boardState = newBoardState;

            Castling = previousState.Castling;
            if (move.PieceType.HasFlag(PieceType.King)) Castling |= move.PieceType.HasFlag(PieceType.White) ? Castling.WhiteKingMoved : Castling.BlackKingMoved;
            if (move.PieceType.HasFlag(PieceType.Rook))
            {
                if (move.PieceType.HasFlag(PieceType.White)) Castling |= move.Source == (7, 7) ? Castling.WhiteKingsideRookMoved : Castling.WhiteQueensideRookMoved;
                else Castling |= move.Source == (0, 7) ? Castling.BlackKingsideRookMoved : Castling.BlackQueensideRookMoved;
            }

            IsWhitesTurn = !previousState.IsWhitesTurn;
            _fullmoveCount = previousState._fullmoveCount + (previousState.IsWhitesTurn ? 0 : 1);
            _halfmovesSinceLastCaptureOrPawnAdvance = IsProgressMade(move) ? 0 : previousState._halfmovesSinceLastCaptureOrPawnAdvance + 1;

            LastMove = move;

            if (LastMove.MoveType == MoveType.EnPassantMove)
                EnPassantTarget = ((LastMove.Source.row + LastMove.Destination.row) / 2, LastMove.Destination.col);
        }

        private static bool IsProgressMade(Move move) =>
            move.MoveType == MoveType.Capture ||
            move.PieceType.HasFlag(PieceType.Pawn);

        public IBoard MakeMove(Move move) => new Board(this, move);

        public PieceType this[int row, int col] => _boardState[row, col];
        public PieceType this[(int row, int col) pos] => _boardState[pos.row, pos.col];
        public bool IsEmptyAt((int row, int col) pos) => _boardState[pos.row, pos.col] == PieceType.None;

        public IEnumerable<Piece> BoardPieces => _boardState.Select((type, pos) => new Piece(type, pos)).Where(p => p.Type != PieceType.None);
        public IEnumerable<Piece> WhitePieces => BoardPieces.Where(p => p.Type.HasFlag(PieceType.White));
        public IEnumerable<Piece> BlackPieces => BoardPieces.Where(p => !p.Type.HasFlag(PieceType.White));

        public IEnumerable<Move> WhiteMoves => throw new NotImplementedException();
        public IEnumerable<Move> BlackMoves => throw new NotImplementedException();

        public bool IsWhitesTurn { get; }
        public Move LastMove { get; }
        public (int row, int col)? EnPassantTarget { get; }

        public static Board NewGame()
        {
            var startingPositionFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
            return new Board(startingPositionFen);
        }

        public override string ToString() =>
            FenNotation.GetFenString(_boardState, LastMove, Castling, _halfmovesSinceLastCaptureOrPawnAdvance, _fullmoveCount);
    }
}
