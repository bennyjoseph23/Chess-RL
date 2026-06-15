namespace ChessRL.Engine;

public class Board
{
    public Bitboard Bitboard;
    public Color SideToMove;
    public byte CastlingRights; 
    public Square EnPassantSquare;
    public int HalfMoveClock;
    public int FullMoveNumber;
    public List<ulong> PositionHistory = new List<ulong>();
    public ulong CurrentHash;

    public Board()
    {
        Bitboard = new Bitboard();
        Reset();
    }

    public bool IsInCheck()
    {
        ulong kingBB = Bitboard.PieceBB[(int)Piece.King] & Bitboard.ColorBB[(int)SideToMove];
        if (kingBB == 0) return false;
        return MoveGenerator.IsSquareAttacked(this, BitboardUtils.LSB(kingBB), 1 - (int)SideToMove);
    }

    public void MakeMove(Move move)
    {
        PositionHistory.Add(CurrentHash);

        Piece piece = Piece.None;
        for (int p = (int)Piece.Pawn; p <= (int)Piece.King; p++)
        {
            if (BitboardUtils.GetBit(Bitboard.PieceBB[p] & Bitboard.ColorBB[(int)SideToMove], move.From))
            {
                piece = (Piece)p;
                break;
            }
        }

        // Incremental Hash Update: Remove piece from 'From'
        CurrentHash ^= Zobrist.GetPieceKey(piece, SideToMove, move.From);

        // Handle Castling
        if (piece == Piece.King && Math.Abs((int)move.From - (int)move.To) == 2)
        {
            Square rFrom, rTo;
            if (move.To == Square.G1) { rFrom = Square.H1; rTo = Square.F1; }
            else if (move.To == Square.C1) { rFrom = Square.A1; rTo = Square.D1; }
            else if (move.To == Square.G8) { rFrom = Square.H8; rTo = Square.F8; }
            else { rFrom = Square.A8; rTo = Square.D8; }

            BitboardUtils.ClearBit(ref Bitboard.PieceBB[(int)Piece.Rook], rFrom);
            BitboardUtils.ClearBit(ref Bitboard.ColorBB[(int)SideToMove], rFrom);
            BitboardUtils.SetBit(ref Bitboard.PieceBB[(int)Piece.Rook], rTo);
            BitboardUtils.SetBit(ref Bitboard.ColorBB[(int)SideToMove], rTo);
            
            CurrentHash ^= Zobrist.GetPieceKey(Piece.Rook, SideToMove, rFrom);
            CurrentHash ^= Zobrist.GetPieceKey(Piece.Rook, SideToMove, rTo);
        }

        BitboardUtils.ClearBit(ref Bitboard.PieceBB[(int)piece], move.From);
        BitboardUtils.ClearBit(ref Bitboard.ColorBB[(int)SideToMove], move.From);

        // Handle Captures
        bool isCapture = false;
        for (int p = (int)Piece.Pawn; p <= (int)Piece.King; p++)
        {
            if (BitboardUtils.GetBit(Bitboard.PieceBB[p] & Bitboard.ColorBB[1 - (int)SideToMove], move.To))
            {
                BitboardUtils.ClearBit(ref Bitboard.PieceBB[p], move.To);
                BitboardUtils.ClearBit(ref Bitboard.ColorBB[1 - (int)SideToMove], move.To);
                CurrentHash ^= Zobrist.GetPieceKey((Piece)p, (Color)(1 - (int)SideToMove), move.To);
                isCapture = true;
                break;
            }
        }

        Piece finalPiece = piece;
        if (piece == Piece.Pawn)
        {
            int rank = (int)move.To / 8;
            if (rank == 0 || rank == 7) finalPiece = Piece.Queen;
            HalfMoveClock = 0; 
        }
        else if (isCapture) HalfMoveClock = 0;
        else HalfMoveClock++;

        BitboardUtils.SetBit(ref Bitboard.PieceBB[(int)finalPiece], move.To);
        BitboardUtils.SetBit(ref Bitboard.ColorBB[(int)SideToMove], move.To);
        
        // Incremental Hash Update: Add piece to 'To'
        CurrentHash ^= Zobrist.GetPieceKey(finalPiece, SideToMove, move.To);

        // Update Castling Rights Hash
        CurrentHash ^= Zobrist.GetCastlingKey(CastlingRights);
        if (piece == Piece.King) CastlingRights &= (byte)(SideToMove == Color.White ? 0xC : 0x3);
        if (move.From == Square.A1 || move.To == Square.A1) CastlingRights &= 0xD;
        if (move.From == Square.H1 || move.To == Square.H1) CastlingRights &= 0xE;
        if (move.From == Square.A8 || move.To == Square.A8) CastlingRights &= 0x7;
        if (move.From == Square.H8 || move.To == Square.H8) CastlingRights &= 0xB;
        CurrentHash ^= Zobrist.GetCastlingKey(CastlingRights);

        if (SideToMove == Color.Black) FullMoveNumber++;
        SideToMove = 1 - SideToMove;
        CurrentHash ^= Zobrist.GetSideKey();
    }

    public bool IsThreefoldRepetition()
    {
        int count = 0;
        foreach (var h in PositionHistory) if (h == CurrentHash) count++;
        return count >= 2; 
    }

    public Board Clone()
    {
        return new Board {
            Bitboard = new Bitboard {
                PieceBB = (ulong[])Bitboard.PieceBB.Clone(),
                ColorBB = (ulong[])Bitboard.ColorBB.Clone()
            },
            SideToMove = SideToMove,
            CastlingRights = CastlingRights,
            EnPassantSquare = EnPassantSquare,
            HalfMoveClock = HalfMoveClock,
            FullMoveNumber = FullMoveNumber,
            PositionHistory = new List<ulong>(PositionHistory),
            CurrentHash = CurrentHash
        };
    }

    public string GetFen()
    {
        string fen = "";
        for (int r = 7; r >= 0; r--)
        {
            int empty = 0;
            for (int f = 0; f < 8; f++)
            {
                int sq = r * 8 + f;
                char pieceChar = ' ';
                for (int p = 1; p <= 6; p++)
                {
                    if (BitboardUtils.GetBit(Bitboard.PieceBB[p], (Square)sq))
                    {
                        bool isWhite = BitboardUtils.GetBit(Bitboard.ColorBB[0], (Square)sq);
                        pieceChar = " PNBRQK"[p];
                        if (!isWhite) pieceChar = char.ToLower(pieceChar);
                        break;
                    }
                }

                if (pieceChar == ' ') empty++;
                else
                {
                    if (empty > 0) { fen += empty; empty = 0; }
                    fen += pieceChar;
                }
            }
            if (empty > 0) fen += empty;
            if (r > 0) fen += "/";
        }

        fen += (SideToMove == Color.White) ? " w " : " b ";
        
        string castling = "";
        if ((CastlingRights & 1) != 0) castling += "K";
        if ((CastlingRights & 2) != 0) castling += "Q";
        if ((CastlingRights & 4) != 0) castling += "k";
        if ((CastlingRights & 8) != 0) castling += "q";
        fen += (castling == "") ? "-" : castling;

        fen += " - "; // En Passant (Simplified)
        fen += $"{HalfMoveClock} {FullMoveNumber}";
        return fen;
    }

    public void Reset()
    {
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
        PositionHistory.Clear();
        CurrentHash = Zobrist.CalculateHash(this);
    }
}
