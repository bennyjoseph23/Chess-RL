using ChessRL.Engine;

bool useDashboard = args.Contains("--dashboard");
Console.WriteLine($"--- Chess-RL Simulator Started (Visuals: {(useDashboard ? "ON" : "OFF")}) ---");

using var messenger = new Messenger("tcp://*:5555");
var searchManager = new SearchManager(messenger) { EnableVisuals = useDashboard };
var board = new Board();

int gameCount = 0;
while (true)
{
    Console.WriteLine($"\nStarting Game #{++gameCount}");
    board.Reset();
    
    int moveCount = 0;
    while (moveCount < 200) // Safety cap for game length
    {
        // 1. Let the AI "Think" using MCTS
        // Use temperature for variety in the opening
        float temperature = (board.FullMoveNumber <= 15) ? 1.0f : 0.1f;
        Move bestMove = searchManager.IterativeDeepening(board, 800, temperature);
        
        if (bestMove.From == bestMove.To) 
        {
            int us = (int)board.SideToMove;
            int them = 1 - us;
            Square kingSq = BitboardUtils.LSB(board.Bitboard.PieceBB[(int)Piece.King] & board.Bitboard.ColorBB[us]);
            
            if (MoveGenerator.IsSquareAttacked(board, kingSq, them))
                Console.WriteLine($"CHECKMATE! {(board.SideToMove == Color.White ? "Black" : "White")} wins.");
            else
                Console.WriteLine("DRAW by Stalemate.");
            break;
        }

        // 2. Apply the move to the board
        board.MakeMove(bestMove);
        moveCount++;

        // 3. Log progress
        string checkStatus = board.IsInCheck() ? " [CHECK!]" : "";
        Console.WriteLine($"Move {moveCount}: {bestMove}{checkStatus} | Hash: {board.CurrentHash:X}");

        // 4. Check for special Draw rules
        if (board.HalfMoveClock >= 100) // 100 half-moves = 50 full moves
        {
            Console.WriteLine("DRAW by Fifty-Move Rule.");
            break;
        }
        if (board.IsThreefoldRepetition())
        {
            Console.WriteLine("DRAW by Threefold Repetition.");
            break;
        }
        messenger.BroadcastEvaluation("game_update", new {
            FullMove = board.FullMoveNumber,
            Side = board.SideToMove.ToString(),
            LastMove = bestMove.ToString(),
            Fen = board.GetFen()
        });

        // Small delay to prevent ZeroMQ from flooding during local dev
        Thread.Sleep(50);
    }

    Console.WriteLine($"Game #{gameCount} finished in {moveCount} moves.");
}
