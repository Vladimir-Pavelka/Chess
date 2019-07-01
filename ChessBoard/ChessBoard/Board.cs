namespace ChessBoard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class Board : IBoard
    {
        private readonly IDictionary<PieceType, string> _fenNotationPieceNames = new Dictionary<PieceType, string> {
                { PieceType.BlackPawn, "p" }, { PieceType.BlackRook, "r" }, { PieceType.BlackKnight, "n" },
                { PieceType.BlackBishop, "b" }, { PieceType.BlackQueen, "q" }, { PieceType.BlackKing, "k" },
                { PieceType.WhitePawn, "P" }, { PieceType.WhiteRook, "R" }, { PieceType.WhiteKnight, "N" },
                { PieceType.WhiteBishop, "B" }, { PieceType.WhiteQueen, "Q" }, { PieceType.WhiteKing, "K" }
        };

        public IEnumerable<Move> WhiteMoves => throw new NotImplementedException();
        public IEnumerable<Move> BlackMoves => throw new NotImplementedException();
        public void ExecuteMove(Move move) => throw new NotImplementedException();

        public PieceType this[int row, int column] { get { return _boardState[row, column]; } }

        private readonly PieceType[,] _boardState = new PieceType[8, 8];
        private bool _whiteKingMoved;
        private bool _whiteKingsideRookMoved;
        private bool _whiteQueensideRookMoved;

        private bool _blackKingMoved;
        private bool _blackKingsideRookMoved;
        private bool _blackQueensideRookMoved;

        private bool _isWhitesMove;

        private Move _lastMove;

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

            return $"{boardContent} {activeColor} {castlingAvailability} {enPassantTargetSquare} {0} {0}";
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
                    if (currentSquare != PieceType.None)
                    {
                        if (emptySquaresCounter > 0)
                        {
                            result.Append(emptySquaresCounter);
                            emptySquaresCounter = 0;
                        }

                        result.Append(_fenNotationPieceNames[currentSquare]);
                        continue;
                    }

                    emptySquaresCounter++;
                }

                if (emptySquaresCounter > 0) result.Append(emptySquaresCounter);
                if (row != 7) result.Append("/");
            }

            return result.ToString();
        }

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
            var file = ('a' + _lastMove.Source.column).ToString();
            var rank = _lastMove.Source.row + _lastMove.Destination.row / 2;

            return $"{file}{rank}";
        }

        private bool IsEnPassantTarget(Move move)
        {
            if (move.PieceType != PieceType.BlackPawn && move.PieceType != PieceType.WhitePawn) return false;
            return Math.Abs(move.Source.row - move.Destination.row) == 2;
        }
    }
}
