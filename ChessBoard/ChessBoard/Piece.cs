namespace ChessBoard
{
    public struct Piece
    {
        public Piece(PieceType type, (int, int) position)
        {
            Type = type;
            Position = position;
        }

        public PieceType Type { get; }
        public (int row, int col) Position { get; }

        public bool IsWhite => PieceType.WhitePiece.HasFlag(Type);
        public bool IsOponentOf(PieceType other) => other != PieceType.None && PieceType.WhitePiece.HasFlag(Type) != PieceType.WhitePiece.HasFlag(other);
        public bool IsAllyOf(PieceType other) => other != PieceType.None && PieceType.WhitePiece.HasFlag(Type) == PieceType.WhitePiece.HasFlag(other);
    }
}
