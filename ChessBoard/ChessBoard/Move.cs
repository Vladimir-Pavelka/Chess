namespace ChessBoard
{
    public struct Move
    {
        public MoveType MoveType { get; }
        public (int row, int column) Source { get; }
        public (int row, int column) Destination { get; }

        public Move(MoveType type, (int row, int column) source, (int row, int column) destination)
        {
            MoveType = type;
            Source = source;
            Destination = destination;
        }
    }
}
