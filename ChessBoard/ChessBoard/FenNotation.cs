namespace ChessBoard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    internal static class FenNotation
    {
        private static readonly IDictionary<PieceType, string> _pieceTypeToFen = new Dictionary<PieceType, string> {
                { PieceType.BlackPawn, "p" }, { PieceType.BlackRook, "r" }, { PieceType.BlackKnight, "n" },
                { PieceType.BlackBishop, "b" }, { PieceType.BlackQueen, "q" }, { PieceType.BlackKing, "k" },
                { PieceType.WhitePawn, "P" }, { PieceType.WhiteRook, "R" }, { PieceType.WhiteKnight, "N" },
                { PieceType.WhiteBishop, "B" }, { PieceType.WhiteQueen, "Q" }, { PieceType.WhiteKing, "K" }
        };

        private static readonly IDictionary<string, PieceType> _fenToPieceType = _pieceTypeToFen.ToDictionary(x => x.Value, x => x.Key);

        public static string GetFenString(PieceType[,] boardState, Move lastMove, CastlingInfo castlingInfo, int halfmovesSinceLastCaptureOrPawnAdvance, int fullmoveCount)
        {
            var boardContent = GetBoardContentFenString(boardState);
            var isWhitesMove = !lastMove.IsWhitesMove;
            var activeColor = isWhitesMove ? "w" : "b";
            var castlingAvailability = GetCastlingAvailabilityFenString(castlingInfo);
            var enPassantTargetSquare = GetEnPassantFenString(lastMove);

            return $"{boardContent} {activeColor} {castlingAvailability} {enPassantTargetSquare} {halfmovesSinceLastCaptureOrPawnAdvance} {fullmoveCount}";
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

        private static string GetCastlingAvailabilityFenString(CastlingInfo castlingInfo)
        {
            var castlingAvailability = string.Empty;
            if (!castlingInfo.WhiteKingMoved)
            {
                if (!castlingInfo.WhiteKingsideRookMoved) castlingAvailability += "K";
                if (!castlingInfo.WhiteQueensideRookMoved) castlingAvailability += "Q";
            }

            if (!castlingInfo.BlackKingMoved)
            {
                if (!castlingInfo.BlackKingsideRookMoved) castlingAvailability += "k";
                if (!castlingInfo.BlackQueensideRookMoved) castlingAvailability += "q";
            }

            return string.IsNullOrEmpty(castlingAvailability) ? "-" : castlingAvailability;
        }

        private static string GetEnPassantFenString(Move lastMove)
        {
            if (lastMove.MoveType != MoveType.EnPassantMove) return "-";
            var file = (char)('a' + lastMove.Source.column);
            var rank = 8 - (lastMove.Source.row + lastMove.Destination.row) / 2;

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

        public static CastlingInfo GetCastlingInfo(string fen)
        {
            // rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1
            var castlingFen = fen.Split(" ")[2];
            var castlingInfo = new CastlingInfo
            {
                WhiteKingMoved = !new[] { "K", "Q" }.Any(castlingFen.Contains),
                WhiteKingsideRookMoved = !castlingFen.Contains("K"),
                WhiteQueensideRookMoved = !castlingFen.Contains("Q"),
                BlackKingMoved = !new[] { "k", "q" }.Any(castlingFen.Contains),
                BlackKingsideRookMoved = !castlingFen.Contains("k"),
                BlackQueensideRookMoved = !castlingFen.Contains("q")
            };

            return castlingInfo;
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

        public static Move GetEnPassantMove(string fen)
        {
            var isWhitesMove = GetIsWhitesMove(fen);
            var enPassantFen = fen.Split(" ")[3];

            var lastMovePlayerPiece = isWhitesMove ? PieceType.BlackPawn : PieceType.WhitePawn;

            if (enPassantFen == "-")
            {
                var dummyColoredMove = new Move(lastMovePlayerPiece, MoveType.Move, (0, 0), (0, 0));
                return dummyColoredMove;
            }
            
            var col = enPassantFen[0] - 'a';
            var row = 8 - int.Parse($"{enPassantFen[1]}");

            var source = (row + (isWhitesMove ? -1 : 1), col);
            var dest = (row + (isWhitesMove ? 1 : -1), col);

            return new Move(lastMovePlayerPiece, MoveType.EnPassantMove, source, dest);
        }
    }
}
