namespace ChessBoard
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    public class MovesGenerator
    {
        private delegate IEnumerable<Move> MovesFactory(Piece p, Board b);

        private readonly IDictionary<PieceType, MovesFactory> _getPieceMovesFnMap;

        public MovesGenerator()
        {
            _getPieceMovesFnMap = new Dictionary<PieceType, MovesFactory>
            {
                { PieceType.Pawn, GetPawnMoves },{ PieceType.White | PieceType.Pawn, GetPawnMoves },
                { PieceType.Knight, GetKnightMoves },{ PieceType.White | PieceType.Knight, GetKnightMoves },
                { PieceType.Bishop, GetBishopMoves },{ PieceType.White | PieceType.Bishop, GetBishopMoves },
                { PieceType.Rook, GetRookMoves },{ PieceType.White | PieceType.Rook, GetRookMoves },
                { PieceType.Queen, GetQueenMoves },{ PieceType.White | PieceType.Queen, GetQueenMoves },
                { PieceType.King, GetKingMoves },{ PieceType.White | PieceType.King, GetKingMoves },
            };
        }

        public IEnumerable<Move> GetAllMoves(Board board)
        {
            var movablePieces = board.IsWhitesTurn ? board.WhitePieces : board.BlackPieces;
            return movablePieces.SelectMany(p => _getPieceMovesFnMap[p.Type](p, board));
        }

        private static readonly PieceType[] _blackPromotionPieces = new[] { PieceType.Knight, PieceType.Bishop, PieceType.Rook, PieceType.Queen };
        private static readonly PieceType[] _whitePromotionPieces = _blackPromotionPieces.Select(pt => pt | PieceType.White).ToArray();

        private readonly int[] _promotionRows = new[] { 0, 7 };
        private const int whitePawnStartRow = 6;
        private const int blackPawnStartRow = 1;

        private IEnumerable<Move> GetPawnMoves(Piece pawn, Board board) =>
            GetSingleFrontStepMoves(pawn, board)
            .Concat(GetDoubleFrontStepMoves(pawn, board))
            .Concat(GetPawnCaptureMoves(pawn, board));

        private IEnumerable<Move> GetSingleFrontStepMoves(Piece pawn, Board board)
        {
            var destRow = pawn.Position.row + (board.IsWhitesTurn ? -1 : 1);
            var dest = (row: destRow, pawn.Position.col);
            if (!board.IsEmptyAt(dest)) yield break;

            if (!_promotionRows.Contains(dest.row))
            {
                yield return new Move(pawn.Type, MoveType.Move, pawn.Position, dest);
                yield break;
            }

            var promotionPieceTypes = board.IsWhitesTurn ? _whitePromotionPieces : _blackPromotionPieces;
            foreach (var move in promotionPieceTypes.Select(pt => new Move(pawn.Type, MoveType.Move, pawn.Position, dest, pt)))
                yield return move;
        }

        private IEnumerable<Move> GetDoubleFrontStepMoves(Piece pawn, Board board)
        {
            var startPosRow = board.IsWhitesTurn ? whitePawnStartRow : blackPawnStartRow;
            var isStartPos = pawn.Position.row == startPosRow;
            if (!isStartPos) yield break;

            var destRow = pawn.Position.row + (board.IsWhitesTurn ? -2 : 2);
            var dest = (row: destRow, pawn.Position.col);
            var fieldInBetween = Avg(pawn.Position, dest);
            if (board.IsEmptyAt(dest) && board.IsEmptyAt(fieldInBetween))
                yield return new Move(pawn.Type, MoveType.EnPassantMove, pawn.Position, dest);
        }

        private readonly (int row, int col)[] _pawnCaptureOffsets = new[] { (0, 1), (0, -1) };
        private IEnumerable<Move> GetPawnCaptureMoves(Piece pawn, Board board)
        {
            var captureRow = pawn.Position.row + (board.IsWhitesTurn ? -1 : 1);
            var captureDestinations = _pawnCaptureOffsets
                .Select(off => (row: captureRow, col: pawn.Position.col + off.col))
                .Where(IsInBoard)
                .Where(dest => dest == board.EnPassantTarget || pawn.IsOponentOf(board[dest]));

            foreach (var dest in captureDestinations)
            {
                if (!_promotionRows.Contains(dest.row))
                {
                    var moveType = dest == board.EnPassantTarget ? MoveType.EnPassantCapture : MoveType.Capture;
                    yield return new Move(pawn.Type, moveType, pawn.Position, dest);
                    continue;
                }

                var promotionPieceTypes = board.IsWhitesTurn ? _whitePromotionPieces : _blackPromotionPieces;
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
            var isKingWhite = board.IsWhitesTurn;
            var kingMove = isKingWhite ? Castling.WhiteKingMoved : Castling.BlackKingMoved;
            if (board.Castling.HasFlag(kingMove)) yield break;

            var kingsideRookMove = isKingWhite ? Castling.WhiteKingsideRookMoved : Castling.BlackKingsideRookMoved;
            var hasKingsideRookMoved = board.Castling.HasFlag(kingsideRookMove);
            if (!hasKingsideRookMoved)
            {
                var dest = (row: king.Position.row, col: king.Position.col + 2);
                if (board.IsEmptyAt((dest.row, dest.col - 1)) &&
                    board.IsEmptyAt(dest) &&
                    !IsAttacked(king.Position, isKingWhite, board) &&
                    !IsAttacked((dest.row, dest.col - 1), isKingWhite, board) &&
                    !IsAttacked(dest, isKingWhite, board))
                    yield return new Move(king.Type, MoveType.Castle, king.Position, dest);
            }

            var queensideRookMove = isKingWhite ? Castling.WhiteQueensideRookMoved : Castling.BlackQueensideRookMoved;
            var hasQueensideRookMoved = board.Castling.HasFlag(queensideRookMove);
            if (!hasQueensideRookMoved)
            {
                var dest = (row: king.Position.row, col: king.Position.col - 2);
                if (board.IsEmptyAt((dest.row, dest.col - 1)) &&
                    board.IsEmptyAt(dest) &&
                    board.IsEmptyAt((dest.row, dest.col + 1)) &&
                    !IsAttacked(king.Position, isKingWhite, board) &&
                    !IsAttacked((dest.row, dest.col + 1), isKingWhite, board) &&
                    !IsAttacked(dest, isKingWhite, board))
                    yield return new Move(king.Type, MoveType.Castle, king.Position, dest);
            }
        }

        private IEnumerable<Move> ScanDirection((int row, int col) directionalOffset, int maxMovesInAnyDirection, Piece rayPiece, Board board)
        {
            var destinations = Enumerable.Range(1, maxMovesInAnyDirection)
                .Select(q => (row: q * directionalOffset.row + rayPiece.Position.row, col: q * directionalOffset.col + rayPiece.Position.col))
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

        private const PieceType N = PieceType.Knight | PieceType.White;
        private const PieceType S = PieceType.StraightRay;
        private const PieceType D = PieceType.DiagonalRay;
        private const PieceType X = S | PieceType.King;
        private const PieceType Y = D | PieceType.Pawn | PieceType.King;

        private static readonly PieceType[,] _attacksBitMap = new PieceType[,]
        {   //7 6 5 4 3 2 1 0 1 2 3 4 5 6 7
            { D,0,0,0,0,0,0,S,0,0,0,0,0,0,D}, // 7
            { 0,D,0,0,0,0,0,S,0,0,0,0,0,D,0}, // 6
            { 0,0,D,0,0,0,0,S,0,0,0,0,D,0,0}, // 5
            { 0,0,0,D,0,0,0,S,0,0,0,D,0,0,0}, // 4
            { 0,0,0,0,D,0,0,S,0,0,D,0,0,0,0}, // 3
            { 0,0,0,0,0,D,N,S,N,D,0,0,0,0,0}, // 2
            { 0,0,0,0,0,N,Y,X,Y,N,0,0,0,0,0}, // 1
            { S,S,S,S,S,S,X,0,X,S,S,S,S,S,S}, // 0
            { 0,0,0,0,0,N,Y,X,Y,N,0,0,0,0,0}, // 1
            { 0,0,0,0,0,D,N,S,N,D,0,0,0,0,0}, // 2
            { 0,0,0,0,D,0,0,S,0,0,D,0,0,0,0}, // 3
            { 0,0,0,D,0,0,0,S,0,0,0,D,0,0,0}, // 4
            { 0,0,D,0,0,0,0,S,0,0,0,0,D,0,0}, // 5
            { 0,D,0,0,0,0,0,S,0,0,0,0,0,D,0}, // 6
            { D,0,0,0,0,0,0,S,0,0,0,0,0,0,D}, // 7
        };

        private static readonly (int row, int col) _attacksBitMapCenter = (7, 7);

        public static bool IsAttacked((int row, int col) defPos, bool isDefenderWhite, Board board)
        {
            var attackers = isDefenderWhite ? board.BlackPieces : board.WhitePieces;
            return attackers.Any(attacker =>
            {
                var defenderBitMapPosRow = defPos.row - attacker.Position.row + _attacksBitMapCenter.row;
                var defenderBitMapPosCol = defPos.col - attacker.Position.col + _attacksBitMapCenter.col;
                var isInLineOfSight = _attacksBitMap[defenderBitMapPosRow, defenderBitMapPosCol].HasFlag(attacker.Type);
                if (!isInLineOfSight) return false;
                if (PieceType.RayPiece.HasFlag(attacker.Type)) return !IsRayBlocked(defPos, attacker.Position, board);
                if (!attacker.Type.HasFlag(PieceType.Pawn)) return true;
                var isInFrontOfPawn = isDefenderWhite && attacker.Position.row < defPos.row ||
                        !isDefenderWhite && attacker.Position.row > defPos.row;

                return isInFrontOfPawn;
            });
        }

        private static bool IsRayBlocked((int row, int col) a, (int row, int col) b, Board board) =>
            GetPositionsInbetween(a, b).Any(p => !board.IsEmptyAt(p));

        public static IEnumerable<(int row, int col)> GetPositionsInbetween((int row, int col) a, (int row, int col) b)
        {
            var stepsInbetween = Math.Max(Math.Abs(a.row - b.row), Math.Abs(a.col - b.col)) - 1;
            var rowRange = a.row == b.row ? Enumerable.Repeat(a.row, stepsInbetween) : GetExclusiveRange(a.row, b.row);
            var colRange = a.col == b.col ? Enumerable.Repeat(a.col, stepsInbetween) : GetExclusiveRange(a.col, b.col);

            return rowRange.Zip(colRange, (row, col) => (row, col));
        }

        private static IEnumerable<int> GetExclusiveRange(int a, int b) =>
            Enumerable.Range(Math.Min(a, b) + 1, Math.Abs(a - b) - 1);
    }
}
