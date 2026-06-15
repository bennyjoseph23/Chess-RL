namespace ChessRL.Engine;

public static class Zobrist
{
    private static readonly ulong[,] PieceSquareKeys = new ulong[12, 64];
    private static readonly ulong SideToMoveKey;
    private static readonly ulong[] CastlingKeys = new ulong[16];
    private static readonly ulong[] EnPassantKeys = new ulong[8];

    static Zobrist()
    {
        Random rng = new Random(42); 
        for (int i = 0; i < 12; i++)
            for (int j = 0; j < 64; j++)
                PieceSquareKeys[i, j] = NextUlong(rng);

        SideToMoveKey = NextUlong(rng);
        for (int i = 0; i < 16; i++) CastlingKeys[i] = NextUlong(rng);
        for (int i = 0; i < 8; i++) EnPassantKeys[i] = NextUlong(rng);
    }

    private static ulong NextUlong(Random rng)
    {
        byte[] buffer = new byte[8];
        rng.NextBytes(buffer);
        return BitConverter.ToUInt64(buffer, 0);
    }

    public static ulong GetPieceKey(Piece piece, Color color, Square sq)
    {
        if (piece == Piece.None || sq == Square.None) return 0;
        int index = (int)piece - 1;
        if (color == Color.Black) index += 6;
        return PieceSquareKeys[index, (int)sq];
    }

    public static ulong GetCastlingKey(int rights) => CastlingKeys[rights];
    public static ulong GetSideKey() => SideToMoveKey;

    public static ulong CalculateHash(Board board)
    {
        ulong hash = 0;
        for (int piece = (int)Piece.Pawn; piece <= (int)Piece.King; piece++)
        {
            ulong whiteBB = board.Bitboard.PieceBB[piece] & board.Bitboard.ColorBB[(int)Color.White];
            while (whiteBB != 0)
            {
                Square sq = BitboardUtils.LSB(whiteBB);
                hash ^= GetPieceKey((Piece)piece, Color.White, sq);
                whiteBB &= whiteBB - 1;
            }

            ulong blackBB = board.Bitboard.PieceBB[piece] & board.Bitboard.ColorBB[(int)Color.Black];
            while (blackBB != 0)
            {
                Square sq = BitboardUtils.LSB(blackBB);
                hash ^= GetPieceKey((Piece)piece, Color.Black, sq);
                blackBB &= blackBB - 1;
            }
        }

        if (board.SideToMove == Color.Black) hash ^= SideToMoveKey;
        hash ^= CastlingKeys[board.CastlingRights];
        if (board.EnPassantSquare != Square.None)
            hash ^= EnPassantKeys[(int)board.EnPassantSquare % 8];

        return hash;
    }
}
