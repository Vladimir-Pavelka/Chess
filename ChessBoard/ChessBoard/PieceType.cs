namespace ChessBoard
{
    using System;

    [Flags]
    public enum PieceType
    {
        None = 0,
        WhitePawn = 1,
        WhiteKnight = 2,
        WhiteBishop = 4,
        WhiteRook = 8,
        WhiteQueen = 16,
        WhiteKing = 32,
        BlackPawn = 64,
        BlackKnight = 128,
        BlackBishop = 256,
        BlackRook = 512,
        BlackQueen = 1024,
        BlackKing = 2048,

        WhitePiece = WhitePawn | WhiteKnight | WhiteBishop | WhiteRook | WhiteQueen | WhiteKing,
        BlackPiece = BlackPawn | BlackKnight | BlackBishop | BlackRook | BlackQueen | BlackKing,
    }
}
