namespace ChessBoard
{
    using System;
    using System.Collections.Generic;

    public class Board : IBoard
    {
        private readonly PieceType[,] _boardState = new PieceType[8, 8];

        private readonly bool _whiteKingMoved;
        private readonly bool _whiteKingsideRookMoved;
        private readonly bool _whiteQueensideRookMoved;

        private readonly bool _blackKingMoved;
        private readonly bool _blackKingsideRookMoved;
        private readonly bool _blackQueensideRookMoved;

        private readonly bool _isWhitesMove = true;
        private readonly int _fullmoveCount = 1;
        private readonly int _halfmovesSinceLastCaptureOrPawnAdvance = 0;

        private Move _lastMove;

        public Board(string fen)
        {
            _boardState = FenNotation.GetBoardContent(fen);

            var castlingInfo = FenNotation.GetCastlingInfo(fen);
            _whiteKingMoved = castlingInfo.WhiteKingMoved;
            _whiteKingsideRookMoved = castlingInfo.WhiteKingsideRookMoved;
            _whiteQueensideRookMoved = castlingInfo.WhiteQueensideRookMoved;

            _blackKingMoved = castlingInfo.BlackKingMoved;
            _blackKingsideRookMoved = castlingInfo.BlackKingsideRookMoved;
            _blackQueensideRookMoved = castlingInfo.BlackQueensideRookMoved;

            _isWhitesMove = FenNotation.GetIsWhitesMove(fen);
            _fullmoveCount = FenNotation.GetFullmoveCount(fen);
            _halfmovesSinceLastCaptureOrPawnAdvance = FenNotation.GetHalfmovesSinceProgressCount(fen);

            _lastMove = FenNotation.GetEnPassantMove(fen);
        }

        private Board(Board previousState, Move move)
        {
            var newBoardState = (PieceType[,])previousState._boardState.Clone();
            newBoardState[move.Source.row, move.Source.column] = PieceType.None;
            newBoardState[move.Destination.row, move.Destination.column] = move.PromotedInto ?? move.PieceType;

            if (move.MoveType == MoveType.EnPassantCapture)
            {
                var capturedPawnPos = (row: move.Source.row, column: move.Destination.column);
                newBoardState[capturedPawnPos.row, capturedPawnPos.column] = PieceType.None;
            }

            if (move.MoveType == MoveType.Castle)
            {
                var isKingsideCastle = move.Destination.column > move.Source.column;
                var row = move.Source.row;
                var rookSourcePos = isKingsideCastle ? (row: row, column: 7) : (row: row, column: 0);
                var rookDestPos = isKingsideCastle ? (row: row, column: 5) : (row: row, column: 3);
                newBoardState[rookDestPos.row, rookDestPos.column] = newBoardState[rookSourcePos.row, rookSourcePos.column];
                newBoardState[rookSourcePos.row, rookSourcePos.column] = PieceType.None;
            }

            _boardState = newBoardState;

            _whiteKingMoved = previousState._whiteKingMoved || move.PieceType == PieceType.WhiteKing;
            _whiteKingsideRookMoved = previousState._whiteKingsideRookMoved || move.PieceType == PieceType.WhiteRook && move.Source == (7, 7);
            _whiteQueensideRookMoved = previousState._whiteQueensideRookMoved || move.PieceType == PieceType.WhiteRook && move.Source == (7, 0);

            _blackKingMoved = previousState._blackKingMoved || move.PieceType == PieceType.BlackKing;
            _blackKingsideRookMoved = previousState._blackKingsideRookMoved || move.PieceType == PieceType.BlackRook && move.Source == (0, 7);
            _blackQueensideRookMoved = previousState._blackQueensideRookMoved || move.PieceType == PieceType.BlackRook && move.Source == (0, 0);

            _isWhitesMove = !previousState._isWhitesMove;
            _fullmoveCount = previousState._fullmoveCount + (previousState._isWhitesMove ? 0 : 1);
            _halfmovesSinceLastCaptureOrPawnAdvance = IsProgressMade(move) ? 0 : previousState._halfmovesSinceLastCaptureOrPawnAdvance + 1;

            _lastMove = move;
        }

        private static bool IsProgressMade(Move move) =>
            move.MoveType == MoveType.Capture ||
            move.PieceType == PieceType.BlackPawn ||
            move.PieceType == PieceType.WhitePawn;

        public IEnumerable<Move> WhiteMoves => throw new NotImplementedException();
        public IEnumerable<Move> BlackMoves => throw new NotImplementedException();

        public IBoard MakeMove(Move move) => new Board(this, move);

        public PieceType this[int row, int column] { get { return _boardState[row, column]; } }

        public static Board NewGame()
        {
            var startingPositionFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
            return new Board(startingPositionFen);
        }

        public override string ToString()
        {
            var castlingInfo = new CastlingInfo
            {
                WhiteKingMoved = _whiteKingMoved,
                WhiteKingsideRookMoved = _whiteKingsideRookMoved,
                WhiteQueensideRookMoved = _whiteQueensideRookMoved,
                BlackKingMoved = _blackKingMoved,
                BlackKingsideRookMoved = _blackKingsideRookMoved,
                BlackQueensideRookMoved = _blackQueensideRookMoved
            };

            return FenNotation.GetFenString(_boardState, _lastMove, castlingInfo, _halfmovesSinceLastCaptureOrPawnAdvance, _fullmoveCount);
        }
    }
}
