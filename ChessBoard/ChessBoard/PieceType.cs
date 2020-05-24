namespace ChessBoard
{
    using System;

    [Flags]
    public enum PieceType : byte
    {
        None = 0,
        White = 1,
        Pawn = 2,
        Knight = 4,
        Bishop = 8,
        Rook = 16,
        Queen = 32,
        King = 64,

        DiagonalRay = Bishop | Queen | White,
        StraightRay = Rook | Queen | White,
        RayPiece = DiagonalRay | StraightRay,
    }
}
