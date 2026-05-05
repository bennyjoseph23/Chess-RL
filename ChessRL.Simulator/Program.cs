using ChessRL.Engine;

Console.WriteLine("--- Chess-RL Simulator Started ---");

using var messenger = new Messenger("tcp://*:5555");
var searchManager = new SearchManager(messenger);
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
        // 400 simulations is a good starting point for training
        Move bestMove = searchManager.IterativeDeepening(board, 400);
        
        if (bestMove.From == bestMove.To) // No legal moves found (Draw or Mate)
        {
            Console.WriteLine("Game Over: No legal moves.");
            break;
        }

        // 2. Apply the move to the board
        board.MakeMove(bestMove);
        moveCount++;

        // 3. Log progress
        Console.WriteLine($"Move {moveCount}: {bestMove} | Hash: {Zobrist.CalculateHash(board):X}");

        // 4. Send the updated board to the Dashboard
        messenger.BroadcastEvaluation("game_update", new {
            FullMove = board.FullMoveNumber,
            Side = board.SideToMove.ToString(),
            LastMove = bestMove.ToString(),
            Fen = "Board state update" // In a real app, convert board to FEN
        });

        // Small delay to prevent ZeroMQ from flooding during local dev
        Thread.Sleep(50);
    }

    Console.WriteLine($"Game #{gameCount} finished in {moveCount} moves.");
}
