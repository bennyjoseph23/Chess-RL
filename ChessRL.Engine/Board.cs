namespace ChessRL.Engine;

public class Board
{
    public Bitboard Bitboard;
    public Color SideToMove;
    public byte CastlingRights; // 4 bits: WK, WQ, BK, BQ
    public Square EnPassantSquare;
    public int HalfMoveClock;
    public int FullMoveNumber;

    public Board()
    {
        Bitboard = new Bitboard();
        Reset();
    }

    public void MakeMove(Move move)
    {
        // 1. Get piece being moved
        Piece piece = Piece.None;
        for (int p = (int)Piece.Pawn; p <= (int)Piece.King; p++)
        {
            if (BitboardUtils.GetBit(Bitboard.PieceBB[p] & Bitboard.ColorBB[(int)SideToMove], move.From))
            {
                piece = (Piece)p;
                break;
            }
        }

        // 2. Remove from 'From' square
        BitboardUtils.ClearBit(ref Bitboard.PieceBB[(int)piece], move.From);
        BitboardUtils.ClearBit(ref Bitboard.ColorBB[(int)SideToMove], move.From);

        // 3. Handle Captures
        for (int p = (int)Piece.Pawn; p <= (int)Piece.King; p++)
        {
            if (BitboardUtils.GetBit(Bitboard.PieceBB[p] & Bitboard.ColorBB[1 - (int)SideToMove], move.To))
            {
                BitboardUtils.ClearBit(ref Bitboard.PieceBB[p], move.To);
                BitboardUtils.ClearBit(ref Bitboard.ColorBB[1 - (int)SideToMove], move.To);
                HalfMoveClock = 0; // Reset on capture
                break;
            }
        }

        // 4. Place on 'To' square
        BitboardUtils.SetBit(ref Bitboard.PieceBB[(int)piece], move.To);
        BitboardUtils.SetBit(ref Bitboard.ColorBB[(int)SideToMove], move.To);

        // 5. Update state
        if (piece == Piece.Pawn) HalfMoveClock = 0;
        else HalfMoveClock++;

        if (SideToMove == Color.Black) FullMoveNumber++;
        SideToMove = 1 - SideToMove;
        
        // TODO: Update Castling and EP rights
    }

    public void Reset()
    {
        // Initial setup for standard chess
        Bitboard.PieceBB[(int)Piece.Pawn] = 0x00FF00000000FF00UL;
        Bitboard.PieceBB[(int)Piece.Knight] = 0x4200000000000042UL;
        Bitboard.PieceBB[(int)Piece.Bishop] = 0x2400000000000024UL;
        Bitboard.PieceBB[(int)Piece.Rook] = 0x8100000000000081UL;
        Bitboard.PieceBB[(int)Piece.Queen] = 0x0800000000000008UL;
        Bitboard.PieceBB[(int)Piece.King] = 0x1000000000000010UL;

        Bitboard.ColorBB[(int)Color.White] = 0x000000000000FFFFUL;
        Bitboard.ColorBB[(int)Color.Black] = 0xFFFF000000000000UL;

        SideToMove = Color.White;
        CastlingRights = 0xF;
        EnPassantSquare = Square.None;
        HalfMoveClock = 0;
        FullMoveNumber = 1;
    }
}
