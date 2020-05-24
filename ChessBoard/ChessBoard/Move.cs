namespace ChessBoard
{
    public struct Move
    {
        public PieceType PieceType { get; }
        public MoveType MoveType { get; }
        public (int row, int col) Source { get; }
        public (int row, int col) Destination { get; }
        public PieceType? PromotedInto { get; }

        public Move(PieceType pieceType, MoveType moveType, (int row, int col) source, (int row, int col) destination, PieceType? promotedInto = null)
        {
            PieceType = pieceType;
            MoveType = moveType;
            Source = source;
            Destination = destination;
            PromotedInto = promotedInto;
        }
    }
}
