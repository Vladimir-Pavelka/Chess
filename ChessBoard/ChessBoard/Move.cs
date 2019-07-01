namespace ChessBoard
{
    public struct Move
    {
        public PieceType PieceType { get; }
        public MoveType MoveType { get; }
        public (int row, int column) Source { get; }
        public (int row, int column) Destination { get; }

        public Move(PieceType pieceType, MoveType moveType, (int row, int column) source, (int row, int column) destination)
        {
            PieceType = pieceType;
            MoveType = moveType;
            Source = source;
            Destination = destination;
        }
    }
}
