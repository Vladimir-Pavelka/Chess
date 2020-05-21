namespace ChessBoard
{
    public class CastlingInfo
    {
        public bool WhiteKingMoved { get; set; }
        public bool WhiteKingsideRookMoved { get; set; }
        public bool WhiteQueensideRookMoved { get; set; }

        public bool BlackKingMoved { get; set; }
        public bool BlackKingsideRookMoved { get; set; }
        public bool BlackQueensideRookMoved { get; set; }
    }
}
