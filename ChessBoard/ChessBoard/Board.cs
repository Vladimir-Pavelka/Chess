namespace ChessBoard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

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

        private Board()
        {
        }

        private Board(Board previousState, Move move)
        {
            var newBoardState = (PieceType[,])previousState._boardState.Clone();
            newBoardState[move.Source.row, move.Source.column] = PieceType.None;
            newBoardState[move.Destination.row, move.Destination.column] = move.PieceType;

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

        public IBoard ExecuteMove(Move move) =>
            new Board(this, move);

        public PieceType this[int row, int column] { get { return _boardState[row, column]; } }

        private void InitializeBoard()
        {
            _boardState[0, 0] = _boardState[0, 7] = PieceType.BlackRook;
            _boardState[0, 1] = _boardState[0, 6] = PieceType.BlackKnight;
            _boardState[0, 2] = _boardState[0, 5] = PieceType.BlackBishop;
            _boardState[0, 3] = PieceType.BlackQueen;
            _boardState[0, 4] = PieceType.BlackKing;
            Enumerable.Range(0, 8).ToList().ForEach(i => _boardState[1, i] = PieceType.BlackPawn);

            _boardState[7, 0] = _boardState[7, 7] = PieceType.WhiteRook;
            _boardState[7, 1] = _boardState[7, 6] = PieceType.WhiteKnight;
            _boardState[7, 2] = _boardState[7, 5] = PieceType.WhiteBishop;
            _boardState[7, 3] = PieceType.WhiteQueen;
            _boardState[7, 4] = PieceType.WhiteKing;
            Enumerable.Range(0, 8).ToList().ForEach(i => _boardState[6, i] = PieceType.WhitePawn);
        }

        public static Board Empty() => new Board();

        public static Board NewGame()
        {
            var board = new Board();
            board.InitializeBoard();
            return board;
        }

        public override string ToString()
        {
            var boardContent = GetBoardContentFenString();
            var activeColor = _isWhitesMove ? "w" : "b";
            var castlingAvailability = GetCastlingAvailabilityFenString();
            var enPassantTargetSquare = GetEnPassantFenString();

            return $"{boardContent} {activeColor} {castlingAvailability} {enPassantTargetSquare} {_halfmovesSinceLastCaptureOrPawnAdvance} {_fullmoveCount}";
        }

        private string GetBoardContentFenString()
        {
            var result = new StringBuilder();

            for (var row = 0; row < 8; row++)
            {
                var emptySquaresCounter = 0;

                for (var col = 0; col < 8; col++)
                {
                    var currentSquare = _boardState[row, col];
                    if (currentSquare == PieceType.None)
                    {
                        emptySquaresCounter++;
                        continue;
                    }

                    if (emptySquaresCounter > 0)
                    {
                        result.Append(emptySquaresCounter);
                        emptySquaresCounter = 0;
                    }

                    result.Append(_fenNotationPieceNames[currentSquare]);
                }

                if (emptySquaresCounter > 0) result.Append(emptySquaresCounter);
                if (row != 7) result.Append("/");
            }

            return result.ToString();
        }

        private static readonly IDictionary<PieceType, string> _fenNotationPieceNames = new Dictionary<PieceType, string> {
                { PieceType.BlackPawn, "p" }, { PieceType.BlackRook, "r" }, { PieceType.BlackKnight, "n" },
                { PieceType.BlackBishop, "b" }, { PieceType.BlackQueen, "q" }, { PieceType.BlackKing, "k" },
                { PieceType.WhitePawn, "P" }, { PieceType.WhiteRook, "R" }, { PieceType.WhiteKnight, "N" },
                { PieceType.WhiteBishop, "B" }, { PieceType.WhiteQueen, "Q" }, { PieceType.WhiteKing, "K" }
        };

        private string GetCastlingAvailabilityFenString()
        {
            var castlingAvailability = string.Empty;
            if (!_whiteKingMoved)
            {
                if (!_whiteKingsideRookMoved) castlingAvailability += "K";
                if (!_whiteQueensideRookMoved) castlingAvailability += "Q";
            }

            if (!_blackKingMoved)
            {
                if (!_blackKingsideRookMoved) castlingAvailability += "k";
                if (!_blackQueensideRookMoved) castlingAvailability += "q";
            }

            return string.IsNullOrEmpty(castlingAvailability) ? "-" : castlingAvailability;
        }

        private string GetEnPassantFenString()
        {
            if (!IsEnPassantTarget(_lastMove)) return "-";
            var file = (char)('a' + _lastMove.Source.column);
            var rank = 8 - (_lastMove.Source.row + _lastMove.Destination.row) / 2;

            return $"{file}{rank}";
        }

        private bool IsEnPassantTarget(Move move)
        {
            if (move.PieceType != PieceType.BlackPawn && move.PieceType != PieceType.WhitePawn) return false;
            var hasMovedByTwo = Math.Abs(move.Source.row - move.Destination.row) == 2;
            return hasMovedByTwo;
        }
    }
}
