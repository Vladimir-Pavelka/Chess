namespace ChessBoard
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    class MovesGenerator
    {
        private delegate IEnumerable<Move> MovesFactory(Piece p, Board b);

        private readonly IDictionary<PieceType, MovesFactory> _getPieceMovesFnMap;

        public MovesGenerator()
        {
            _getPieceMovesFnMap = new Dictionary<PieceType, MovesFactory>
            {
                { PieceType.WhitePawn, GetPawnMoves }, { PieceType.BlackPawn, GetPawnMoves },
                { PieceType.WhiteKnight, GetKnightMoves }, { PieceType.BlackKnight, GetKnightMoves },
                { PieceType.WhiteBishop, GetBishopMoves }, { PieceType.BlackBishop, GetBishopMoves },
                { PieceType.WhiteRook, GetRookMoves }, { PieceType.BlackRook, GetRookMoves },
                { PieceType.WhiteQueen, GetQueenMoves }, { PieceType.BlackQueen, GetQueenMoves },
                { PieceType.WhiteKing, GetKingMoves }, { PieceType.BlackKing, GetKingMoves },
            };
        }

        public IEnumerable<Move> GetAllMoves(Board board)
        {
            var movablePieces = board.IsWhitesTurn ? board.WhitePieces : board.BlackPieces;
            return movablePieces.SelectMany(p => _getPieceMovesFnMap[p.Type](p, board));
        }

        private readonly PieceType[] _whitePromotionPieces = new[] { PieceType.WhiteKnight, PieceType.WhiteBishop, PieceType.WhiteRook, PieceType.WhiteQueen };
        private readonly PieceType[] _blackPromotionPieces = new[] { PieceType.BlackKnight, PieceType.BlackBishop, PieceType.BlackRook, PieceType.BlackQueen };
        private readonly int[] _promotionRows = new[] { 0, 7 };
        private const int whitePawnStartRow = 6;
        private const int blackPawnStartRow = 1;

        private IEnumerable<Move> GetPawnMoves(Piece pawn, Board board) =>
            GetSingleFrontStepMoves(pawn, board)
            .Concat(GetDoubleFrontStepMoves(pawn, board))
            .Concat(GetPawnCaptureMoves(pawn, board));

        private IEnumerable<Move> GetSingleFrontStepMoves(Piece pawn, Board board)
        {
            var destRow = pawn.Position.row + (pawn.IsWhite ? -1 : 1);
            var dest = (row: destRow, pawn.Position.col);
            if (!board.IsEmptyAt(dest)) yield break;

            if (!_promotionRows.Contains(dest.row))
            {
                yield return new Move(pawn.Type, MoveType.Move, pawn.Position, dest);
                yield break;
            }

            var promotionPieceTypes = pawn.IsWhite ? _whitePromotionPieces : _blackPromotionPieces;
            foreach (var move in promotionPieceTypes.Select(pt => new Move(pawn.Type, MoveType.Move, pawn.Position, dest, pt)))
                yield return move;
        }

        private IEnumerable<Move> GetDoubleFrontStepMoves(Piece pawn, Board board)
        {
            var startPosRow = pawn.IsWhite ? whitePawnStartRow : blackPawnStartRow;
            var isStartPos = pawn.Position.row == startPosRow;
            if (!isStartPos) yield break;

            var destRow = pawn.Position.row + (pawn.IsWhite ? -2 : 2);
            var dest = (row: destRow, pawn.Position.col);
            var fieldInBetween = Avg(pawn.Position, dest);
            if (board.IsEmptyAt(dest) && board.IsEmptyAt(fieldInBetween))
                yield return new Move(pawn.Type, MoveType.EnPassantMove, pawn.Position, dest);
        }

        private readonly (int row, int col)[] _pawnCaptureOffsets = new[] { (0, 1), (0, -1) };
        private IEnumerable<Move> GetPawnCaptureMoves(Piece pawn, Board board)
        {
            var captureRow = pawn.Position.row + (pawn.IsWhite ? -1 : 1);
            var captureDestinations = _pawnCaptureOffsets
                .Select(off => (row: captureRow, col: pawn.Position.col + off.col))
                .Where(IsInBoard)
                .Where(dest => dest == board.EnPassantTarget || !board.IsEmptyAt(dest) && pawn.IsOponentOf(board[dest]));

            foreach (var dest in captureDestinations)
            {
                if (!_promotionRows.Contains(dest.row))
                {
                    var moveType = dest == board.EnPassantTarget ? MoveType.EnPassantCapture : MoveType.Capture;
                    yield return new Move(pawn.Type, moveType, pawn.Position, dest);
                    yield break;
                }

                var promotionPieceTypes = pawn.IsWhite ? _whitePromotionPieces : _blackPromotionPieces;
                foreach (var move in promotionPieceTypes.Select(pt => new Move(pawn.Type, MoveType.Capture, pawn.Position, dest, pt)))
                    yield return move;
            }
        }

        private readonly (int row, int col)[] _knightMoveOffsets = new[] { (-2, 1), (-1, 2), (1, 2), (2, 1), (2, -1), (1, -2), (-1, -2), (-2, -1) };
        private IEnumerable<Move> GetKnightMoves(Piece knight, Board board)
        {
            return _knightMoveOffsets
                .Select(off => (knight.Position.row + off.row, knight.Position.col + off.col))
                .Where(IsInBoard)
                .Where(dest => board.IsEmptyAt(dest) || knight.IsOponentOf(board[dest]))
                .Select(dest => new Move(knight.Type, board.IsEmptyAt(dest) ? MoveType.Move : MoveType.Capture, knight.Position, dest));
        }

        private static readonly (int row, int col)[] _bishopMoveOffsets = new[] { (-1, 1), (1, 1), (1, -1), (-1, -1) };
        private IEnumerable<Move> GetBishopMoves(Piece bishop, Board board) =>
            _bishopMoveOffsets.SelectMany(off => ScanDirection(off, 7, bishop, board));

        private static readonly (int row, int col)[] _rookMoveOffsets = new[] { (-1, 0), (0, 1), (1, 0), (0, -1) };
        private IEnumerable<Move> GetRookMoves(Piece rook, Board board) =>
            _rookMoveOffsets.SelectMany(off => ScanDirection(off, 7, rook, board));

        private static readonly (int row, int col)[] _queenMoveOffsets = _bishopMoveOffsets.Concat(_rookMoveOffsets).ToArray();
        private IEnumerable<Move> GetQueenMoves(Piece queen, Board board) =>
            _queenMoveOffsets.SelectMany(off => ScanDirection(off, 7, queen, board));

        private IEnumerable<Move> GetKingMoves(Piece king, Board board)
        {
            var standardMoves = _queenMoveOffsets.SelectMany(off => ScanDirection(off, 1, king, board));
            var castlingMoves = GetKingCastlingMoves(king, board);

            return standardMoves.Concat(castlingMoves);
        }

        private IEnumerable<Move> GetKingCastlingMoves(Piece king, Board board)
        {
            var hasKingMoved = king.IsWhite ? board.WhiteKingMoved : board.BlackKingMoved;
            if (!hasKingMoved) yield break;

            var hasKingsideRookMoved = king.IsWhite ? board.WhiteKingsideRookMoved : board.BlackKingsideRookMoved;
            if (!hasKingsideRookMoved)
            {
                var dest = (row: king.Position.row, col: king.Position.col + 2);
                if (board.IsEmptyAt((dest.row, dest.col - 1)) &&
                    board.IsEmptyAt(dest) &&
                    !IsAttacked(king.Position, king.Type, board) &&
                    !IsAttacked((dest.row, dest.col - 1), king.Type, board) &&
                    !IsAttacked(dest, king.Type, board))
                    yield return new Move(king.Type, MoveType.Castle, king.Position, dest);
            }

            var hasQueensideRookMoved = king.IsWhite ? board.WhiteQueensideRookMoved : board.BlackQueensideRookMoved;
            if (!hasQueensideRookMoved)
            {
                var dest = (row: king.Position.row, col: king.Position.col - 2);
                if (board.IsEmptyAt((dest.row, dest.col - 1)) &&
                    board.IsEmptyAt(dest) &&
                    board.IsEmptyAt((dest.row, dest.col + 1)) &&
                    !IsAttacked(king.Position, king.Type, board) &&
                    !IsAttacked((dest.row, dest.col + 1), king.Type, board) &&
                    !IsAttacked(dest, king.Type, board))
                    yield return new Move(king.Type, MoveType.Castle, king.Position, dest);
            }
        }

        private IEnumerable<Move> ScanDirection((int row, int col) directionalOffset, int maxMovesInAnyDirection, Piece rayPiece, Board board)
        {
            var destinations = Enumerable.Range(1, maxMovesInAnyDirection)
                .Select(q => (row: directionalOffset.row + q * rayPiece.Position.row, col: directionalOffset.col + q * rayPiece.Position.col))
                .TakeWhile(IsInBoard);

            foreach (var dest in destinations)
            {
                var target = board[dest];
                if (rayPiece.IsAllyOf(target)) yield break;
                var moveType = rayPiece.IsOponentOf(target) ? MoveType.Capture : MoveType.Move;
                yield return new Move(rayPiece.Type, moveType, rayPiece.Position, dest);
            }
        }

        private static bool IsInBoard((int row, int col) pos) =>
            pos.row >= 0 && pos.row < 8 &&
            pos.col >= 0 && pos.col < 8;

        private static (int row, int col) Avg((int row, int col) a, (int row, int col) b) =>
            ((a.row + b.row) / 2, (b.col + b.col) / 2);

        private static bool IsAttacked((int row, int col) pos, PieceType defender, Board board) => false;
    }
}
