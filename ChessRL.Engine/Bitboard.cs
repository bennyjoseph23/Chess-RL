using System.Runtime.CompilerServices;

namespace ChessRL.Engine;

public static class BitboardUtils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetBit(ref ulong bb, Square square) => bb |= 1UL << (int)square;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ClearBit(ref ulong bb, Square square) => bb &= ~(1UL << (int)square);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GetBit(ulong bb, Square square) => (bb & (1UL << (int)square)) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int PopCount(ulong bb) => System.Numerics.BitOperations.PopCount(bb);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square LSB(ulong bb) => (Square)System.Numerics.BitOperations.TrailingZeroCount(bb);
}

public struct Bitboard
{
    public ulong[] PieceBB; // [Piece]
    public ulong[] ColorBB; // [Color]

    public Bitboard()
    {
        PieceBB = new ulong[7];
        ColorBB = new ulong[2];
    }
}
