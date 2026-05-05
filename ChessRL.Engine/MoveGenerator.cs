using System.Runtime.CompilerServices;

namespace ChessRL.Engine;

public struct Move
{
    public Square From;
    public Square To;
    public Piece Promotion;
    public byte Flags; 

    public Move(Square from, Square to, byte flags = 0, Piece promotion = Piece.None)
    {
        From = from;
        To = to;
        Flags = flags;
        Promotion = promotion;
    }

    public override string ToString() => $"{From}{To}".ToLower();
}

public static class MoveGenerator
{
    public static int GenerateMoves(Board board, Span<Move> moveList)
    {
        int moveCount = 0;
        ulong occupancy = board.Bitboard.ColorBB[0] | board.Bitboard.ColorBB[1];
        ulong enemyPieces = board.Bitboard.ColorBB[1 - (int)board.SideToMove];
        ulong emptySquares = ~occupancy;
        int us = (int)board.SideToMove;

        // 1. Pawns
        ulong pawns = board.Bitboard.PieceBB[(int)Piece.Pawn] & board.Bitboard.ColorBB[us];
        if (board.SideToMove == Color.White)
        {
            // Pushes
            ulong singlePush = (pawns << 8) & emptySquares;
            AddMoves(ref moveCount, moveList, singlePush, 8);
            
            // Captures (White captures black)
            ulong attacksWest = (pawns << 7) & board.Bitboard.ColorBB[(int)Color.Black] & ~PrecomputedData.FileH;
            ulong attacksEast = (pawns << 9) & board.Bitboard.ColorBB[(int)Color.Black] & ~PrecomputedData.FileA;
            AddMoves(ref moveCount, moveList, attacksWest, 7);
            AddMoves(ref moveCount, moveList, attacksEast, 9);
        }
        else
        {
            // Pushes
            ulong singlePush = (pawns >> 8) & emptySquares;
            AddMoves(ref moveCount, moveList, singlePush, -8);

            // Captures (Black captures white)
            ulong attacksWest = (pawns >> 9) & board.Bitboard.ColorBB[(int)Color.White] & ~PrecomputedData.FileH;
            ulong attacksEast = (pawns >> 7) & board.Bitboard.ColorBB[(int)Color.White] & ~PrecomputedData.FileA;
            AddMoves(ref moveCount, moveList, attacksWest, -9);
            AddMoves(ref moveCount, moveList, attacksEast, -7);
        }

        // 2. Knights
        ulong knights = board.Bitboard.PieceBB[(int)Piece.Knight] & board.Bitboard.ColorBB[us];
        while (knights != 0)
        {
            Square from = BitboardUtils.LSB(knights);
            ulong attacks = PrecomputedData.KnightAttacks[(int)from] & ~board.Bitboard.ColorBB[us];
            AddMovesFromSquare(ref moveCount, moveList, from, attacks);
            knights &= knights - 1;
        }

        // 3. Sliding Pieces (Using Ray attacks logic)
        GenerateSlidingMoves(board, Piece.Rook, us, occupancy, ref moveCount, moveList);
        GenerateSlidingMoves(board, Piece.Bishop, us, occupancy, ref moveCount, moveList);
        GenerateSlidingMoves(board, Piece.Queen, us, occupancy, ref moveCount, moveList);

        // 4. King
        ulong king = board.Bitboard.PieceBB[(int)Piece.King] & board.Bitboard.ColorBB[us];
        if (king != 0)
        {
            Square from = BitboardUtils.LSB(king);
            ulong attacks = PrecomputedData.KingAttacks[(int)from] & ~board.Bitboard.ColorBB[us];
            AddMovesFromSquare(ref moveCount, moveList, from, attacks);
        }

        return moveCount;
    }

    private static void GenerateSlidingMoves(Board board, Piece piece, int us, ulong occupancy, ref int count, Span<Move> list)
    {
        ulong sliders = board.Bitboard.PieceBB[(int)piece] & board.Bitboard.ColorBB[us];
        while (sliders != 0)
        {
            Square from = BitboardUtils.LSB(sliders);
            ulong attacks = GetSlidingAttacks(piece, from, occupancy) & ~board.Bitboard.ColorBB[us];
            AddMovesFromSquare(ref count, list, from, attacks);
            sliders &= sliders - 1;
        }
    }

    private static ulong GetSlidingAttacks(Piece piece, Square sq, ulong occupancy)
    {
        if (piece == Piece.Rook) return MagicBitboards.GetRookAttacks((int)sq, occupancy);
        if (piece == Piece.Bishop) return MagicBitboards.GetBishopAttacks((int)sq, occupancy);
        if (piece == Piece.Queen) return MagicBitboards.GetRookAttacks((int)sq, occupancy) | MagicBitboards.GetBishopAttacks((int)sq, occupancy);
        return 0;
    }

    private static void AddMoves(ref int count, Span<Move> list, ulong destinations, int offset)
    {
        while (destinations != 0)
        {
            Square to = BitboardUtils.LSB(destinations);
            list[count++] = new Move((Square)((int)to - offset), to);
            destinations &= destinations - 1;
        }
    }

    private static void AddMovesFromSquare(ref int count, Span<Move> list, Square from, ulong destinations)
    {
        while (destinations != 0)
        {
            Square to = BitboardUtils.LSB(destinations);
            list[count++] = new Move(from, to);
            destinations &= destinations - 1;
        }
    }
}
