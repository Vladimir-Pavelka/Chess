namespace ChessBoard
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    internal static class FenNotation
    {
        private static readonly IDictionary<PieceType, string> _pieceTypeToFen = new Dictionary<PieceType, string> {
                { PieceType.Pawn, "p" }, { PieceType.Rook, "r" }, { PieceType.Knight, "n" },
                { PieceType.Bishop, "b" }, { PieceType.Queen, "q" }, { PieceType.King, "k" },
                { PieceType.Pawn | PieceType.White, "P" }, { PieceType.Rook | PieceType.White, "R" }, { PieceType.Knight | PieceType.White, "N" },
                { PieceType.Bishop | PieceType.White, "B" }, { PieceType.Queen | PieceType.White, "Q" }, { PieceType.King | PieceType.White, "K" }
        };

        private static readonly IDictionary<string, PieceType> _fenToPieceType = _pieceTypeToFen.ToDictionary(x => x.Value, x => x.Key);

        public static string GetFenString(Board board)
        {
            var boardContent = GetBoardContentFenString(board.BoardState);
            var activeColor = board.IsWhitesTurn ? "w" : "b";
            var castlingAvailability = GetCastlingAvailabilityFenString(board.Castling);
            var enPassantTargetSquare = GetEnPassantFenString(board.EnPassantTarget);

            return $"{boardContent} {activeColor} {castlingAvailability} {enPassantTargetSquare} {board.HalfmovesSinceLastCaptureOrPawnAdvance} {board.FullmoveCount}";
        }

        private static string GetBoardContentFenString(PieceType[,] boardState)
        {
            var result = new StringBuilder();

            for (var row = 0; row < 8; row++)
            {
                var emptySquaresCounter = 0;

                for (var col = 0; col < 8; col++)
                {
                    var currentSquare = boardState[row, col];
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

                    result.Append(_pieceTypeToFen[currentSquare]);
                }

                if (emptySquaresCounter > 0) result.Append(emptySquaresCounter);
                if (row != 7) result.Append("/");
            }

            return result.ToString();
        }

        private static string GetCastlingAvailabilityFenString(Castling castlingInfo)
        {
            var castlingAvailability = string.Empty;
            if (!castlingInfo.HasFlag(Castling.WhiteKingMoved))
            {
                if (!castlingInfo.HasFlag(Castling.WhiteKingsideRookMoved)) castlingAvailability += "K";
                if (!castlingInfo.HasFlag(Castling.WhiteQueensideRookMoved)) castlingAvailability += "Q";
            }

            if (!castlingInfo.HasFlag(Castling.BlackKingMoved))
            {
                if (!castlingInfo.HasFlag(Castling.BlackKingsideRookMoved)) castlingAvailability += "k";
                if (!castlingInfo.HasFlag(Castling.BlackQueensideRookMoved)) castlingAvailability += "q";
            }

            return string.IsNullOrEmpty(castlingAvailability) ? "-" : castlingAvailability;
        }

        private static string GetEnPassantFenString((int row, int col)? enPassantTarget)
        {
            if (!enPassantTarget.HasValue) return "-";
            var file = (char)('a' + enPassantTarget.Value.col);
            var rank = 8 - enPassantTarget.Value.row;

            return $"{file}{rank}";
        }

        public static PieceType[,] GetBoardContent(string fen)
        {
            // rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1
            var boardContentFen = fen.Split(" ")[0];
            var rows = boardContentFen.Split("/");

            var boardContent = new PieceType[8, 8];

            for (var rowIdx = 0; rowIdx <= 7; rowIdx++)
            {
                var colIdx = 0;
                foreach (var value in rows[rowIdx])
                {
                    if (_fenToPieceType.TryGetValue($"{value}", out var pieceType))
                    {
                        boardContent[rowIdx, colIdx] = pieceType;
                        colIdx++;
                        continue;
                    }

                    var skipCount = int.Parse($"{value}");
                    colIdx += skipCount;
                }
            }

            return boardContent;
        }

        public static Castling GetCastlingInfo(string fen)
        {
            // rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1
            var castlingFen = fen.Split(" ")[2];
            return new (bool shouldSetFlag, Castling flag)[] {
                (!new[] { "K", "Q" }.Any(castlingFen.Contains), Castling.WhiteKingMoved),
                (!castlingFen.Contains("K"), Castling.WhiteKingsideRookMoved),
                (!castlingFen.Contains("Q"), Castling.WhiteQueensideRookMoved),
                (!new[] { "k", "q" }.Any(castlingFen.Contains),Castling.BlackKingMoved),
                (!castlingFen.Contains("k"), Castling.BlackKingsideRookMoved),
                (!castlingFen.Contains("q"), Castling.BlackQueensideRookMoved)
            }
            .Where(x => x.shouldSetFlag)
            .Select(x => x.flag)
            .Concat(new[] { default(Castling) })
            .Aggregate((castling, flag) => castling | flag);
        }

        public static bool GetIsWhitesMove(string fen)
        {
            var playersMoveFen = fen.Split(" ")[1];
            return playersMoveFen == "w";
        }

        public static int GetHalfmovesSinceProgressCount(string fen)
        {
            var halfMovesFen = fen.Split(" ")[4];
            return int.Parse(halfMovesFen);
        }

        public static int GetFullmoveCount(string fen)
        {
            var fullMovesFen = fen.Split(" ")[5];
            return int.Parse(fullMovesFen);
        }

        public static (int row, int col)? GetEnPassantTarget(string fen)
        {
            var enPassantFen = fen.Split(" ")[3];
            if (enPassantFen == "-") return null;

            var col = enPassantFen[0] - 'a';
            var row = 8 - int.Parse($"{enPassantFen[1]}");
            return (row, col);
        }
    }
}
