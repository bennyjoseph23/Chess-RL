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
            // In a real implementation, we would apply the move to a new board state
            // For now, we scaffold the child node
            Children.Add(new MCTSNode(null, moves[i], this, priors[i]));
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
