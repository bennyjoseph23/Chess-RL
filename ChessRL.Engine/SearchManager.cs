namespace ChessRL.Engine;

public class SearchManager
{
    private readonly Messenger _messenger;
    private const float Cpuct = 1.41f;

    public SearchManager(Messenger messenger)
    {
        _messenger = messenger;
    }

    public Move IterativeDeepening(Board board, int simulations)
    {
        MCTSNode root = new MCTSNode(board);
        Span<Move> movesBuffer = stackalloc Move[256];

        for (int i = 0; i < simulations; i++)
        {
            MCTSNode leaf = SelectLeaf(root);
            
            // 1. Request Evaluation from Python Brain
            _messenger.BroadcastEvaluation("search_state", new {
                hash = Zobrist.CalculateHash(board),
                bitboards = board.Bitboard.PieceBB.Select((bb, i) => {
                    // Combine piece BB with color BBs for 12 planes
                    ulong white = bb & board.Bitboard.ColorBB[0];
                    ulong black = bb & board.Bitboard.ColorBB[1];
                    return new[] { white, black };
                }).SelectMany(x => x).ToArray(),
                side_to_move = (int)board.SideToMove,
                castling_rights = (int)board.CastlingRights,
                en_passant = (int)board.EnPassantSquare
            });

            // 2. Expand and Backpropagate (Scaffolded)
            if (!leaf.IsExpanded)
            {
                int moveCount = MoveGenerator.GenerateMoves(leaf.State ?? board, movesBuffer);
                if (moveCount > 0)
                {
                    float[] dummyPriors = new float[moveCount];
                    for (int j = 0; j < moveCount; j++) dummyPriors[j] = 1.0f / moveCount;
                    leaf.Expand(movesBuffer.Slice(0, moveCount), dummyPriors);
                }
            }
            
            leaf.Backpropagate(0.5f); // Dummy evaluation
        }

        return GetBestMove(root);
    }

    private MCTSNode SelectLeaf(MCTSNode node)
    {
        MCTSNode current = node;
        while (current.IsExpanded)
        {
            current = current.Select(Cpuct);
        }
        return current;
    }

    private Move GetBestMove(MCTSNode root)
    {
        MCTSNode bestChild = null;
        int maxVisits = -1;

        foreach (var child in root.Children)
        {
            if (child.VisitCount > maxVisits)
            {
                maxVisits = child.VisitCount;
                bestChild = child;
            }
        }

        return bestChild?.Move ?? default;
    }
}
