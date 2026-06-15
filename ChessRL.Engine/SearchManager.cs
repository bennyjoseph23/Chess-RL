namespace ChessRL.Engine;

public class SearchManager
{
    private readonly Messenger _messenger;
    private const float Cpuct = 1.41f;
    private static readonly Random Rng = new Random();
    public bool EnableVisuals { get; set; } = true;

    public SearchManager(Messenger messenger)
    {
        _messenger = messenger;
    }

    public Move IterativeDeepening(Board board, int simulations, float temperature = 1.0f)
    {
        MCTSNode root = new MCTSNode(board);
        Span<Move> movesBuffer = stackalloc Move[256];

        for (int i = 0; i < simulations; i++)
        {
            MCTSNode leaf = SelectLeaf(root);
            Board leafState = leaf.State ?? board; 
            
            if (EnableVisuals && i % 100 == 0) // Only broadcast every 100 sims if enabled
            {
                _messenger.BroadcastEvaluation("search_state", new {
                    hash = leafState.CurrentHash,
                    bitboards = leafState.Bitboard.PieceBB.Select((bb, j) => {
                        ulong white = bb & leafState.Bitboard.ColorBB[0];
                        ulong black = bb & leafState.Bitboard.ColorBB[1];
                        return new[] { white, black };
                    }).SelectMany(x => x).ToArray(),
                    side_to_move = (int)leafState.SideToMove,
                    castling_rights = (int)leafState.CastlingRights,
                    en_passant = (int)leafState.EnPassantSquare,
                    value = leaf.GetValue(),
                    visits = leaf.VisitCount
                });
            }

            if (!leaf.IsExpanded)
            {
                int moveCount = MoveGenerator.GenerateLegalMoves(leafState, movesBuffer);
                if (moveCount > 0)
                {
                    float[] priors = new float[moveCount];
                    float totalWeight = 0;

                    for (int j = 0; j < moveCount; j++) {
                        Board temp = leafState.Clone();
                        temp.MakeMove(movesBuffer[j]);
                        
                        float eval = Heuristics.Evaluate(temp);
                        // Temperature scale for priors (sharpen focus)
                        float weight = (float)Math.Exp(eval * 8.0); 
                        
                        priors[j] = weight;
                        totalWeight += weight;
                    }
                    for (int j = 0; j < moveCount; j++) priors[j] /= totalWeight;

                    leaf.Expand(movesBuffer.Slice(0, moveCount), priors);

                    // Proven state propagation
                    bool allLoss = true;
                    foreach(var child in leaf.Children)
                    {
                        float val = Heuristics.Evaluate(child.State);
                        if (val == 1.0f) { leaf.Backpropagate(1.0f); i = simulations; allLoss = false; break; }
                        if (val > -1.0f) allLoss = false;
                    }
                    if (allLoss && leaf.Children.Count > 0) leaf.Backpropagate(-1.0f);
                }
                else
                {
                    bool inCheck = leafState.IsInCheck();
                    leaf.Backpropagate(inCheck ? -1.0f : 0.0f);
                    continue;
                }
            }
            
            leaf.Backpropagate(Heuristics.Evaluate(leafState));
        }

        return GetBestMove(root, temperature);
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

    private Move GetBestMove(MCTSNode root, float temperature)
    {
        if (root.Children.Count == 0) return default;

        // If temperature is near zero, pick max visits (Deterministic)
        if (temperature < 0.1f)
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

        // Stochastic selection: P(a) = N(a)^(1/tau) / Sum(N(i)^(1/tau))
        double[] probabilities = new double[root.Children.Count];
        double sum = 0;
        for (int i = 0; i < root.Children.Count; i++)
        {
            probabilities[i] = Math.Pow(root.Children[i].VisitCount, 1.0 / temperature);
            sum += probabilities[i];
        }

        double r = Rng.NextDouble() * sum;
        double currentSum = 0;
        for (int i = 0; i < root.Children.Count; i++)
        {
            currentSum += probabilities[i];
            if (r <= currentSum) return root.Children[i].Move;
        }

        return root.Children[0].Move;
    }
}
