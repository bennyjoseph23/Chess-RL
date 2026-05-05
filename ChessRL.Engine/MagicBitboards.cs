namespace ChessRL.Engine;

public static class MagicBitboards
{
    private static readonly ulong[] BishopMasks = new ulong[64];
    private static readonly ulong[] RookMasks = new ulong[64];
    
    private static readonly ulong[][] BishopTable = new ulong[64][];
    private static readonly ulong[][] RookTable = new ulong[64][];

    private static readonly int[] BishopBits = {
        6, 5, 5, 5, 5, 5, 5, 6,
        5, 5, 5, 5, 5, 5, 5, 5,
        5, 5, 7, 7, 7, 7, 5, 5,
        5, 5, 7, 9, 9, 7, 5, 5,
        5, 5, 7, 9, 9, 7, 5, 5,
        5, 5, 7, 7, 7, 7, 5, 5,
        5, 5, 5, 5, 5, 5, 5, 5,
        6, 5, 5, 5, 5, 5, 5, 6
    };

    private static readonly int[] RookBits = {
        12, 11, 11, 11, 11, 11, 11, 12,
        11, 10, 10, 10, 10, 10, 10, 11,
        11, 10, 10, 10, 10, 10, 10, 11,
        11, 10, 10, 10, 10, 10, 10, 11,
        11, 10, 10, 10, 10, 10, 10, 11,
        11, 10, 10, 10, 10, 10, 10, 11,
        11, 10, 10, 10, 10, 10, 10, 11,
        12, 11, 11, 11, 11, 11, 11, 12
    };

    // Pre-generated Magic Numbers (using standard well-known seeds)
    private static readonly ulong[] BishopMagics = {
        0x40040822862081UL, 0x40810a4108000UL, 0x200800840042001UL, 0x401600c8061UL, 0x10022024040a0403UL, 0x402800805016014UL, 0x805020440100401UL, 0x22020410410101UL,
        0x201440401000a02UL, 0x80044010802000UL, 0x4100810a00880a1UL, 0x4012010101201UL, 0x22220a04104202UL, 0x4010882008102UL, 0x802100402011008UL, 0x41040820010022UL,
        0x20010080800a1UL, 0x202101002011008UL, 0x810100100108UL, 0x4040011001100UL, 0x84010010044002UL, 0x212000401010201UL, 0x200110010080UL, 0x241100a0802200UL,
        0x408a040121102UL, 0x10024021040204UL, 0x1000840502040102UL, 0x8021100a1202204UL, 0x4408010040200UL, 0x4008200410420UL, 0x11020401101008UL, 0x1000800102081UL,
        0x2020102040201UL, 0x401100212022004UL, 0x1021100104100UL, 0x420080a2202008UL, 0x110410a00440aUL, 0x402011040402UL, 0x800402002041UL, 0x1102021004020202UL,
        0x2080042001100UL, 0x4104004020100UL, 0x240204081010UL, 0x4020120800102UL, 0x80204008010102UL, 0x400202100102UL, 0x20201004221UL, 0x4200a0400801UL,
        0x8021044084201UL, 0x40021040200a2UL, 0x400a042104002UL, 0x10110020400202UL, 0x404100110a2UL, 0x21010040101UL, 0x440a0080402UL, 0x4410104020104UL,
        0x102220404104102UL, 0x10100204020840UL, 0x111221111120102UL, 0x40480a010041UL, 0x11010010202UL, 0x8401100801UL, 0x4080820800401UL, 0x10002105004002UL
    };

    private static readonly ulong[] RookMagics = {
        0xa8002c000108020UL, 0x6c00044802408UL, 0x100080200041UL, 0x1500031001104UL, 0x1100004081UL, 0x100000100202UL, 0x10004200201200UL, 0x40100080200001UL,
        0x840410400010100UL, 0x40001000200040UL, 0x2000a0020004010UL, 0x20008020004001UL, 0x20082200010088UL, 0x4100003001001UL, 0x40100400100108UL, 0x40001020401001UL,
        0x81000200200040UL, 0x400100002000102UL, 0x1000802041005UL, 0x10010020100408UL, 0x1100100032008UL, 0x202000802000100UL, 0x110020004102UL, 0x82110080004001UL,
        0x40008801000041UL, 0x10100010200004UL, 0x1010042004400UL, 0x10080020001001UL, 0x200200110003UL, 0x101008020200010UL, 0x210100040001UL, 0x40081082000001UL,
        0x41003001000401UL, 0x11002000200102UL, 0x20400110002UL, 0x200510003001UL, 0x4020048100121UL, 0x4001002010102UL, 0x820112200010001UL, 0x400280010000401UL,
        0x8201000410002UL, 0x10100810102UL, 0x40021000100902UL, 0x10210040100041UL, 0x402010010102UL, 0x40400011010001UL, 0x11005010001101UL, 0x1010010020200UL,
        0x2001008100020042UL, 0x20010108020110UL, 0x8204000100102UL, 0x400401010102UL, 0x2100100008041UL, 0x10011040010002UL, 0x80102000081101UL, 0x4100000802011UL,
        0x2001100010011041UL, 0x2001100004200011UL, 0x410080004008001UL, 0x41011000210402UL, 0x401004001101UL, 0x4000400101201UL, 0x4004002011041UL, 0x40a00030401101UL
    };

    static MagicBitboards()
    {
        Initialize();
    }

    private static void Initialize()
    {
        for (int sq = 0; sq < 64; sq++)
        {
            BishopMasks[sq] = CreateBishopMask(sq);
            RookMasks[sq] = CreateRookMask(sq);

            BishopTable[sq] = new ulong[1 << BishopBits[sq]];
            RookTable[sq] = new ulong[1 << RookBits[sq]];

            PopulateTable(sq, true);
            PopulateTable(sq, false);
        }
    }

    private static void PopulateTable(int sq, bool bishop)
    {
        ulong mask = bishop ? BishopMasks[sq] : RookMasks[sq];
        int bits = bishop ? BishopBits[sq] : RookBits[sq];
        ulong magic = bishop ? BishopMagics[sq] : RookMagics[sq];
        ulong[] table = bishop ? BishopTable[sq] : RookTable[sq];

        int combinations = 1 << bits;
        for (int i = 0; i < combinations; i++)
        {
            ulong occupancy = GetOccupancyFromIndex(i, mask);
            ulong index = (occupancy * magic) >> (64 - bits);
            table[index] = GenerateAttackRaw(sq, occupancy, bishop);
        }
    }

    public static ulong GetBishopAttacks(int sq, ulong occupancy)
    {
        ulong occ = occupancy & BishopMasks[sq];
        ulong index = (occ * BishopMagics[sq]) >> (64 - BishopBits[sq]);
        return BishopTable[sq][index];
    }

    public static ulong GetRookAttacks(int sq, ulong occupancy)
    {
        ulong occ = occupancy & RookMasks[sq];
        ulong index = (occ * RookMagics[sq]) >> (64 - RookBits[sq]);
        return RookTable[sq][index];
    }

    // Helper functions for initialization
    private static ulong CreateBishopMask(int sq)
    {
        ulong mask = 0;
        int r = sq / 8, f = sq % 8;
        for (int i = 1; r + i < 7 && f + i < 7; i++) mask |= 1UL << (sq + i * 9);
        for (int i = 1; r + i < 7 && f - i > 0; i++) mask |= 1UL << (sq + i * 7);
        for (int i = 1; r - i > 0 && f + i < 7; i++) mask |= 1UL << (sq - i * 7);
        for (int i = 1; r - i > 0 && f - i > 0; i++) mask |= 1UL << (sq - i * 9);
        return mask;
    }

    private static ulong CreateRookMask(int sq)
    {
        ulong mask = 0;
        int r = sq / 8, f = sq % 8;
        for (int i = r + 1; i < 7; i++) mask |= 1UL << (i * 8 + f);
        for (int i = r - 1; i > 0; i--) mask |= 1UL << (i * 8 + f);
        for (int i = f + 1; i < 7; i++) mask |= 1UL << (r * 8 + i);
        for (int i = f - 1; i > 0; i--) mask |= 1UL << (r * 8 - f + i);
        return mask;
    }

    private static ulong GetOccupancyFromIndex(int index, ulong mask)
    {
        ulong occupancy = 0;
        int bitCount = BitboardUtils.PopCount(mask);
        for (int i = 0; i < bitCount; i++)
        {
            Square sq = BitboardUtils.LSB(mask);
            if ((index & (1 << i)) != 0) occupancy |= 1UL << (int)sq;
            mask &= mask - 1;
        }
        return occupancy;
    }

    private static ulong GenerateAttackRaw(int sq, ulong occupancy, bool bishop)
    {
        ulong attacks = 0;
        int r = sq / 8, f = sq % 8;
        
        int[] dr = bishop ? new[] { 1, 1, -1, -1 } : new[] { 1, -1, 0, 0 };
        int[] df = bishop ? new[] { 1, -1, 1, -1 } : new[] { 0, 0, 1, -1 };

        for (int d = 0; d < 4; d++)
        {
            for (int i = 1; i < 8; i++)
            {
                int nr = r + i * dr[d], nf = f + i * df[d];
                if (nr < 0 || nr >= 8 || nf < 0 || nf >= 8) break;
                ulong bit = 1UL << (nr * 8 + nf);
                attacks |= bit;
                if ((occupancy & bit) != 0) break;
            }
        }
        return attacks;
    }
}
