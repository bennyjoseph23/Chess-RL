namespace ChessRL.Engine;

public static class PrecomputedData
{
    public static readonly ulong[] KnightAttacks = new ulong[64];
    public static readonly ulong[] KingAttacks = new ulong[64];
    public static readonly ulong[,] PawnAttacks = new ulong[2, 64];
    
    // File Masks to prevent wrapping
    public const ulong FileA = 0x0101010101010101UL;
    public const ulong FileH = 0x8080808080808080UL;
    public const ulong FileAB = FileA | (FileA << 1);
    public const ulong FileGH = FileH | (FileH >> 1);

    static PrecomputedData()
    {
        for (int sq = 0; sq < 64; sq++)
        {
            ulong bit = 1UL << sq;

            // Knights
            if ((bit & FileA) == 0) {
                KnightAttacks[sq] |= (bit << 15) | (bit >> 17);
                if ((bit & (FileA << 1)) == 0) KnightAttacks[sq] |= (bit << 6) | (bit >> 10);
            }
            if ((bit & FileH) == 0) {
                KnightAttacks[sq] |= (bit << 17) | (bit >> 15);
                if ((bit & (FileH >> 1)) == 0) KnightAttacks[sq] |= (bit << 10) | (bit >> 6);
            }

            // Kings
            ulong k = (bit << 8) | (bit >> 8); // Up/Down
            if ((bit & FileA) == 0) k |= (bit >> 1) | (bit << 7) | (bit >> 9);
            if ((bit & FileH) == 0) k |= (bit << 1) | (bit << 9) | (bit >> 7);
            KingAttacks[sq] = k;

            // Pawns (White = 0, Black = 1)
            if ((bit & FileA) == 0) {
                PawnAttacks[0, sq] |= (bit << 7);
                PawnAttacks[1, sq] |= (bit >> 9);
            }
            if ((bit & FileH) == 0) {
                PawnAttacks[0, sq] |= (bit << 9);
                PawnAttacks[1, sq] |= (bit >> 7);
            }
        }
    }
}
