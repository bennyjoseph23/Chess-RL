namespace ChessRL.Engine;

public static class Heuristics
{
    private static readonly int[] mg_value = { 0, 82, 337, 365, 477, 1025, 0 };
    private static readonly int[] eg_value = { 0, 94, 281, 297, 512,  936, 0 };

    private static readonly int[] mg_pawn_table = {
          0,   0,   0,   0,   0,   0,   0,   0,
         98, 134,  61,  95,  68, 126,  34, -11,
         -6,   7,  26,  31,  65,  56,  25, -20,
        -14,  13,   6,  21,  23,  12,  17, -23,
        -27,  -2,  -5,  12,  17,   6,  10, -25,
        -26,  -4,  -4, -10,   3,   3,  33, -12,
        -35,  -1, -20, -23, -15,  24,  38, -22,
          0,   0,   0,   0,   0,   0,   0,   0,
    };

    private static readonly int[] eg_pawn_table = {
          0,   0,   0,   0,   0,   0,   0,   0,
        178, 173, 158, 134, 147, 132, 165, 187,
         94, 100,  85,  67,  56,  53,  82,  84,
         32,  24,  13,   5,  -2,   4,  17,  17,
         13,   9,  -3,  -7,  -7,  -8,   3,  -1,
          4,   7,  -6,   1,   0,  -5,  -1,  -8,
         13,   8,   8,  10,  13,   0,   2,  -7,
          0,   0,   0,   0,   0,   0,   0,   0,
    };

    private static readonly int[] mg_knight_table = {
        -105, -21, -58, -33, -17, -28, -19, -23,
         -29, -53, -12,  -3,  -1,  18, -14, -19,
         -23,  -9,  12,  10,  19,  17,  25, -16,
         -13,   4,  16,  13,  28,  19,  21,  -8,
          -9,  17,  19,  53,  37,  69,  18,  22,
         -47,  60,  37,  65,  84, 129,  73,  44,
         -73, -41,  72,  36,  23,  62,   7, -17,
        -167, -89, -34, -49,  61, -97, -15,-107,
    };

    private static readonly int[] eg_knight_table = {
         -29, -51, -23, -15, -22, -18, -50, -64,
         -42, -20, -10,  -5,  -2, -20, -23, -44,
         -23,  -3,  -1,  15,  10,  -3, -20, -22,
         -18,  -6,  16,  25,  16,  17,   4, -18,
         -17,   3,  22,  22,  22,  11,   8, -18,
         -24, -20,  10,   9,  -1,  -9, -19, -41,
         -25,  -8, -25,  -2,  -9, -25, -24, -52,
         -58, -38, -13, -28, -31, -27, -63, -99,
    };

    private static readonly int[] mg_bishop_table = {
         -33,  -3, -14, -21, -13, -12, -39, -21,
           4,  15,  16,   0,   7,  21,  33,   1,
           0,  15,  15,  15,  14,  27,  18,  10,
          -6,  13,  13,  26,  34,  12,  10,   4,
          -4,   5,  19,  50,  37,  37,   7,  -2,
         -16,  37,  43,  40,  35,  50,  37,  -2,
         -26,  16, -18, -13,  30,  59,  18, -47,
         -29,   4, -82, -37, -25, -42,   7,  -8,
    };

    private static readonly int[] eg_bishop_table = {
         -23,  -9, -23,  -5,  -9, -16,  -5, -17,
         -14, -18,  -7,  -1,   4,  -9, -15, -27,
         -12,  -3,   8,  10,  13,   3,  -7, -15,
          -6,   3,  13,  19,   7,  10,  -3,  -9,
          -3,   9,  12,   9,  14,  10,   3,   2,
           2,  -8,   0,  -1,  -2,   6,   0,   4,
          -8,  -4,   7, -12,  -3, -13,  -4, -14,
         -14, -21, -11,  -8,  -7,  -9, -17, -24,
    };

    private static readonly int[] mg_rook_table = {
         -19, -13,   1,  17,  16,   7, -37, -26,
         -44, -16, -20,  -9,  -1,  11,  -6, -71,
         -45, -25, -16, -17,   3,   0,  -5, -33,
         -36, -26, -12,  -1,   9,  -7,   6, -23,
         -24, -11,   7,  26,  24,  35,  -8, -20,
          -5,  19,  26,  36,  17,  45,  61,  16,
          27,  32,  58,  62,  80,  67,  26,  44,
          32,  42,  32,  51,  63,   9,  31,  43,
    };

    private static readonly int[] eg_rook_table = {
          -9,   2,   3,  -1,  -5, -13,   4, -20,
          -6,  -6,   0,   2,  -9,  -9, -11,  -3,
          -4,   0,  -5,  -1,  -7, -12,  -8, -16,
           3,   5,   8,   4,  -5,  -6,  -8, -11,
           4,   3,  13,   1,   2,   1,  -1,   2,
           7,   7,   7,   5,   4,  -3,  -5,  -3,
          11,  13,  13,  11,  -3,   3,   8,   3,
          13,  10,  18,  15,  12,  12,   8,   5,
    };

    private static readonly int[] mg_queen_table = {
          -1, -18,  -9,  10, -15, -25, -31, -50,
         -35,  -8,  11,   2,   8,  15,  -3,   1,
         -14,   2, -11,  -2,  -5,   2,  14,   5,
          -9, -26,  -9, -10,  -2,  -4,   3,  -3,
         -27, -27, -16, -16,  -1,  17,  -2,   1,
         -13, -17,   7,   8,  29,  56,  47,  57,
         -24, -39,  -5,   1, -16,  57,  28,  54,
         -28,   0,  29,  12,  59,  44,  43,  45,
    };

    private static readonly int[] eg_queen_table = {
         -33, -28, -22, -43,  -5, -32, -20, -41,
         -22, -23, -30, -16, -16, -23, -36, -32,
         -16, -27,  15,   6,   9,  17,  10,   5,
         -18,  28,  19,  47,  31,  34,  39,  23,
           3,  22,  24,  45,  57,  40,  57,  36,
         -20,   6,   9,  49,  47,  35,  19,   9,
         -17,  20,  32,  41,  58,  25,  30,   0,
          -9,  22,  22,  27,  27,  19,  10,  20,
    };

    private static readonly int[] mg_king_table = {
         -15,  36,  12, -54,   8, -28,  24,  14,
           1,   7,  -8, -64, -43, -16,   9,   8,
         -14, -14, -22, -46, -44, -30, -15, -27,
         -49,  -1, -27, -39, -46, -44, -33, -51,
         -17, -20, -12, -27, -30, -25, -14, -36,
          -9,  24,   2, -16, -20,   6,  22, -22,
          29,  -1, -20,  -7,  -8,  -4, -38, -29,
         -65,  23,  16, -15, -56, -34,   2,  13,
    };

    private static readonly int[] eg_king_table = {
         -53, -34, -21, -11, -28, -14, -24, -43,
         -27, -11,   4,  13,  14,   4,  -5, -17,
         -19,  -3,  11,  21,  23,  16,   7,  -9,
         -18,  -4,  21,  24,  27,  23,   9, -11,
          -8,  22,  24,  27,  26,  33,  26,   3,
          10,  17,  23,  15,  20,  45,  44,  13,
         -12,  17,  14,  17,  17,  38,  23,  11,
         -74, -35, -18, -18, -11,  15,   4, -17,
    };

    public static float Evaluate(Board board)
    {
        int us = (int)board.SideToMove;
        int them = 1 - us;

        // Terminal Draw Detection
        if (board.HalfMoveClock >= 100 || board.IsThreefoldRepetition())
        {
            // DRAW CONTEMPT: If we have a significant material advantage, draw is a loss.
            int materialAdv = GetMaterialAdvantage(board);
            if (Math.Abs(materialAdv) > 150) return -0.8f; 
            return 0.0f;
        }

        // Checkmate detection
        if (board.IsInCheck())
        {
            Span<Move> moves = stackalloc Move[256];
            if (MoveGenerator.GenerateLegalMoves(board, moves) == 0) return -1.0f; // Loss for SideToMove
        }

        int score = Quiesce(board, -30000, 30000, 0);

        // NORMALIZE PERSPECTIVE: 
        // Quiesce returns score for 'SideToMove'. 
        // But Evaluate is called at the LEAF of MCTS after a move was made.
        // We want the score for the player WHO JUST MOVED.
        // If SideToMove is now 'us', 'them' just moved. Return value for 'them'.
        // Since Quiesce returns score for 'SideToMove', we negate it.
        float normalized = score / 4000f;
        return Math.Clamp(-normalized, -0.95f, 0.95f);
    }

    private static int GetMaterialAdvantage(Board board)
    {
        int white = 0, black = 0;
        int[] vals = { 0, 100, 300, 300, 500, 900, 0 };
        for(int p=1; p<=5; p++) {
            white += BitboardUtils.PopCount(board.Bitboard.PieceBB[p] & board.Bitboard.ColorBB[0]) * vals[p];
            black += BitboardUtils.PopCount(board.Bitboard.PieceBB[p] & board.Bitboard.ColorBB[1]) * vals[p];
        }
        return (board.SideToMove == Color.White) ? (white - black) : (black - white);
    }

    private static int Quiesce(Board board, int alpha, int beta, int depth)
    {
        int standPat = StaticEvaluation(board);
        if (standPat >= beta) return beta;
        if (alpha < standPat) alpha = standPat;
        if (depth > 4) return alpha; // Depth limit for safety

        Span<Move> moves = stackalloc Move[256];
        int moveCount = MoveGenerator.GenerateLegalMoves(board, moves);

        for (int i = 0; i < moveCount; i++)
        {
            ulong toBit = 1UL << (int)moves[i].To;
            if ((board.Bitboard.ColorBB[1 - (int)board.SideToMove] & toBit) == 0) continue;

            Board temp = board.Clone();
            temp.MakeMove(moves[i]);
            int score = -Quiesce(temp, -beta, -alpha, depth + 1);

            if (score >= beta) return beta;
            if (score > alpha) alpha = score;
        }
        return alpha;
    }

    private static int StaticEvaluation(Board board)
    {
        int mgScore = 0, egScore = 0, gamePhase = 0;
        ulong whitePieces = board.Bitboard.ColorBB[0];
        ulong blackPieces = board.Bitboard.ColorBB[1];

        for (int p = (int)Piece.Pawn; p <= (int)Piece.King; p++)
        {
            ulong whiteBB = board.Bitboard.PieceBB[p] & whitePieces;
            while (whiteBB != 0)
            {
                int sq = (int)BitboardUtils.LSB(whiteBB);
                mgScore += mg_value[p] + GetPSTValue(p, sq, true, true);
                egScore += eg_value[p] + GetPSTValue(p, sq, true, false);
                gamePhase += GetPhaseValue(p);
                whiteBB &= whiteBB - 1;
            }
            ulong blackBB = board.Bitboard.PieceBB[p] & blackPieces;
            while (blackBB != 0)
            {
                int sq = (int)BitboardUtils.LSB(blackBB);
                mgScore -= (mg_value[p] + GetPSTValue(p, sq, false, true));
                egScore -= (eg_value[p] + GetPSTValue(p, sq, false, false));
                gamePhase += GetPhaseValue(p);
                blackBB &= blackBB - 1;
            }
        }
        int mgPhase = Math.Min(24, gamePhase);
        int egPhase = 24 - mgPhase;
        int finalScore = (mgScore * mgPhase + egScore * egPhase) / 24;
        return (board.SideToMove == Color.White) ? finalScore : -finalScore;
    }

    private static int GetPhaseValue(int piece)
    {
        if (piece == (int)Piece.Knight || piece == (int)Piece.Bishop) return 1;
        if (piece == (int)Piece.Rook) return 2;
        if (piece == (int)Piece.Queen) return 4;
        return 0;
    }

    private static int GetPSTValue(int piece, int sq, bool isWhite, bool isMidgame)
    {
        int visualSq = isWhite ? (63 - sq) : sq;
        if (isMidgame)
        {
            switch ((Piece)piece) {
                case Piece.Pawn: return mg_pawn_table[visualSq];
                case Piece.Knight: return mg_knight_table[visualSq];
                case Piece.Bishop: return mg_bishop_table[visualSq];
                case Piece.Rook: return mg_rook_table[visualSq];
                case Piece.Queen: return mg_queen_table[visualSq];
                case Piece.King: return mg_king_table[visualSq];
            }
        } else {
            switch ((Piece)piece) {
                case Piece.Pawn: return eg_pawn_table[visualSq];
                case Piece.Knight: return eg_knight_table[visualSq];
                case Piece.Bishop: return eg_bishop_table[visualSq];
                case Piece.Rook: return eg_rook_table[visualSq];
                case Piece.Queen: return eg_queen_table[visualSq];
                case Piece.King: return eg_king_table[visualSq];
            }
        }
        return 0;
    }
}
