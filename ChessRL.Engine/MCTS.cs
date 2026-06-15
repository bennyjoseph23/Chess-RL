using System.Collections.Concurrent;

namespace ChessRL.Engine;

public class MCTSNode
{
    public Board State { get; }
    public Move Move { get; }
    public MCTSNode Parent { get; }
    public List<MCTSNode> Children { get; }
    
    public int VisitCount { get; set; }
    public float ValueSum { get; set; }
    public float Prior { get; }
    
    public bool IsExpanded => Children.Count > 0;

    public MCTSNode(Board state, Move move = default, MCTSNode parent = null, float prior = 0)
    {
        State = state;
        Move = move;
        Parent = parent;
        Prior = prior;
        Children = new List<MCTSNode>();
    }

    public float GetValue() => VisitCount == 0 ? 0 : ValueSum / VisitCount;

    public MCTSNode Select(float cpuct)
    {
        MCTSNode bestChild = null;
        double bestScore = double.NegativeInfinity;

        foreach (var child in Children)
        {
            // PUCT Formula: Q(s,a) + U(s,a)
            double q = child.GetValue();
            double u = cpuct * child.Prior * Math.Sqrt(VisitCount) / (1 + child.VisitCount);
            double score = q + u;

            if (score > bestScore)
            {
                bestScore = score;
                bestChild = child;
            }
        }

        return bestChild;
    }

    public void Expand(Span<Move> moves, float[] priors)
    {
        for (int i = 0; i < moves.Length; i++)
        {
            // Apply the move to create a NEW board state for the child
            Board nextState = State.Clone();
            nextState.MakeMove(moves[i]);
            Children.Add(new MCTSNode(nextState, moves[i], this, priors[i]));
        }
    }

    public void Backpropagate(float value)
    {
        MCTSNode current = this;
        float v = value;
        while (current != null)
        {
            current.VisitCount++;
            current.ValueSum += v;
            v = -v; // Flip value for opponent
            current = current.Parent;
        }
    }
}
