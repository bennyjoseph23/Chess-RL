import numpy as np
import math

class MCTSNode:
    def __init__(self, state, parent=None, prior=0):
        self.state = state
        self.parent = parent
        self.children = {} # move -> MCTSNode
        self.visit_count = 0
        self.value_sum = 0
        self.prior = prior
        self.state_hash = None # To be used with C# engine

    @property
    def value(self):
        if self.visit_count == 0:
            return 0
        return self.value_sum / self.visit_count

    def select_child(self, c_puct):
        best_score = -float('inf')
        best_move = -1
        best_child = None

        for move, child in self.children.items():
            score = child.value + c_puct * child.prior * math.sqrt(self.visit_count) / (1 + child.visit_count)
            if score > best_score:
                best_score = score
                best_move = move
                best_child = child
        
        return best_move, best_child

    def expand(self, action_probs):
        for move, prob in action_probs:
            if move not in self.children:
                self.children[move] = MCTSNode(None, parent=self, prior=prob)

    def update(self, value):
        self.visit_count += 1
        self.value_sum += value
        if self.parent:
            self.parent.update(-value) # Negate for opponent
