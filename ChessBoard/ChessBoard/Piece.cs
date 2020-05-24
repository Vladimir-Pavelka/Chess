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

        public bool IsWhite => Type.HasFlag(PieceType.White);
        public bool IsOponentOf(PieceType other) => other != PieceType.None && Type.HasFlag(PieceType.White) != other.HasFlag(PieceType.White);
        public bool IsAllyOf(PieceType other) => other != PieceType.None && Type.HasFlag(PieceType.White) == other.HasFlag(PieceType.White);
    }
}
