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
    public static int GenerateLegalMoves(Board board, Span<Move> moveList)
    {
        Span<Move> pseudoMoves = stackalloc Move[256];
        int pseudoCount = GenerateMoves(board, pseudoMoves);
        int legalCount = 0;

        for (int i = 0; i < pseudoCount; i++)
        {
            if (IsMoveLegal(board, pseudoMoves[i]))
            {
                moveList[legalCount++] = pseudoMoves[i];
            }
        }
        return legalCount;
    }

    private static bool IsMoveLegal(Board board, Move move)
    {
        Board tempBoard = board.Clone();
        tempBoard.MakeMove(move);
        
        int us = 1 - (int)tempBoard.SideToMove;
        int them = (int)tempBoard.SideToMove;

        ulong kingBB = tempBoard.Bitboard.PieceBB[(int)Piece.King] & tempBoard.Bitboard.ColorBB[us];
        if (kingBB == 0) return false; 
        
        Square kingSq = BitboardUtils.LSB(kingBB);
        return !IsSquareAttacked(tempBoard, kingSq, them);
    }

    public static bool IsSquareAttacked(Board board, Square sq, int attackerColor)
    {
        if (sq == Square.None) return false; 
        
        ulong occupancy = board.Bitboard.ColorBB[0] | board.Bitboard.ColorBB[1];
        
        // 1. Pawns
        ulong pawnAttacks = PrecomputedData.PawnAttacks[1 - attackerColor, (int)sq];
        if ((pawnAttacks & board.Bitboard.PieceBB[(int)Piece.Pawn] & board.Bitboard.ColorBB[attackerColor]) != 0) return true;

        // 2. Knights
        ulong knightAttacks = PrecomputedData.KnightAttacks[(int)sq];
        if ((knightAttacks & board.Bitboard.PieceBB[(int)Piece.Knight] & board.Bitboard.ColorBB[attackerColor]) != 0) return true;

        // 3. Sliders
        if ((GetSlidingAttacks(Piece.Rook, sq, occupancy) & 
            (board.Bitboard.PieceBB[(int)Piece.Rook] | board.Bitboard.PieceBB[(int)Piece.Queen]) & 
            board.Bitboard.ColorBB[attackerColor]) != 0) return true;

        if ((GetSlidingAttacks(Piece.Bishop, sq, occupancy) & 
            (board.Bitboard.PieceBB[(int)Piece.Bishop] | board.Bitboard.PieceBB[(int)Piece.Queen]) & 
            board.Bitboard.ColorBB[attackerColor]) != 0) return true;

        // 4. King
        ulong kingAttacks = PrecomputedData.KingAttacks[(int)sq];
        if ((kingAttacks & board.Bitboard.PieceBB[(int)Piece.King] & board.Bitboard.ColorBB[attackerColor]) != 0) return true;

        return false;
    }

    public static int GenerateMoves(Board board, Span<Move> moveList)
    {
        int moveCount = 0;
        ulong occupancy = board.Bitboard.ColorBB[0] | board.Bitboard.ColorBB[1];
        ulong emptySquares = ~occupancy;
        int us = (int)board.SideToMove;

        // 1. Pawns
        ulong pawns = board.Bitboard.PieceBB[(int)Piece.Pawn] & board.Bitboard.ColorBB[us];
        if (board.SideToMove == Color.White)
        {
            ulong singlePush = (pawns << 8) & emptySquares;
            AddMoves(ref moveCount, moveList, singlePush, 8);
            ulong doublePush = ((singlePush & 0x0000000000FF0000UL) << 8) & emptySquares;
            AddMoves(ref moveCount, moveList, doublePush, 16);

            ulong attacksWest = (pawns << 7) & board.Bitboard.ColorBB[(int)Color.Black] & ~PrecomputedData.FileH;
            ulong attacksEast = (pawns << 9) & board.Bitboard.ColorBB[(int)Color.Black] & ~PrecomputedData.FileA;
            AddMoves(ref moveCount, moveList, attacksWest, 7);
            AddMoves(ref moveCount, moveList, attacksEast, 9);
        }
        else
        {
            ulong singlePush = (pawns >> 8) & emptySquares;
            AddMoves(ref moveCount, moveList, singlePush, -8);
            ulong doublePush = ((singlePush & 0x00FF000000000000UL) >> 8) & emptySquares;
            AddMoves(ref moveCount, moveList, doublePush, -16);

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

        // 3. Sliders
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

            if (us == (int)Color.White)
            {
                if ((board.CastlingRights & 1) != 0 && (occupancy & 0x60UL) == 0 && !IsSquareAttacked(board, Square.E1, 1) && !IsSquareAttacked(board, Square.F1, 1))
                    moveList[moveCount++] = new Move(Square.E1, Square.G1);
                if ((board.CastlingRights & 2) != 0 && (occupancy & 0xEUL) == 0 && !IsSquareAttacked(board, Square.E1, 1) && !IsSquareAttacked(board, Square.D1, 1))
                    moveList[moveCount++] = new Move(Square.E1, Square.C1);
            }
            else
            {
                if ((board.CastlingRights & 4) != 0 && (occupancy & 0x6000000000000000UL) == 0 && !IsSquareAttacked(board, Square.E8, 0) && !IsSquareAttacked(board, Square.F8, 0))
                    moveList[moveCount++] = new Move(Square.E8, Square.G8);
                if ((board.CastlingRights & 8) != 0 && (occupancy & 0xE00000000000000UL) == 0 && !IsSquareAttacked(board, Square.E8, 0) && !IsSquareAttacked(board, Square.D8, 0))
                    moveList[moveCount++] = new Move(Square.E8, Square.C8);
            }
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
        ulong attacks = 0;
        if (piece == Piece.Rook || piece == Piece.Queen)
        {
            attacks |= RayAttack(sq, occupancy, 8);  // N
            attacks |= RayAttack(sq, occupancy, -8); // S
            attacks |= RayAttack(sq, occupancy, 1);  // E
            attacks |= RayAttack(sq, occupancy, -1); // W
        }
        if (piece == Piece.Bishop || piece == Piece.Queen)
        {
            attacks |= RayAttack(sq, occupancy, 9);  // NE
            attacks |= RayAttack(sq, occupancy, 7);  // NW
            attacks |= RayAttack(sq, occupancy, -9); // SW
            attacks |= RayAttack(sq, occupancy, -7); // SE
        }
        return attacks;
    }

    private static ulong RayAttack(Square sq, ulong occupancy, int direction)
    {
        ulong attacks = 0;
        int current = (int)sq;
        while (true)
        {
            int next = current + direction;
            if (next < 0 || next >= 64) break;
            if (Math.Abs(direction) == 1 || Math.Abs(direction) == 7 || Math.Abs(direction) == 9)
            {
                if (Math.Abs((current % 8) - (next % 8)) > 1) break;
            }
            ulong bit = 1UL << next;
            attacks |= bit;
            if ((occupancy & bit) != 0) break;
            current = next;
        }
        return attacks;
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
