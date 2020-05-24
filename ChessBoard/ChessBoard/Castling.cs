namespace ChessBoard
{
    using System;

    [Flags]
    public enum Castling : byte
    {
        WhiteKingMoved = 1,
        WhiteKingsideRookMoved = 2,
        WhiteQueensideRookMoved = 4,

        BlackKingMoved = 8,
        BlackKingsideRookMoved = 16,
        BlackQueensideRookMoved = 32
    }
}
