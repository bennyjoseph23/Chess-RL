import torch
import torch.optim as optim
from ai.brain import ChessNet
from ai.mcts import MCTSNode
import chess
import numpy as np
from ai.bridge import Bridge
import time
from ai.features import board_to_tensor

class Trainer:
    def __init__(self, model_path=None):
        self.device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
        self.model = ChessNet().to(self.device)
        self.model.eval() # Set to evaluation mode
        if model_path:
            self.model.load_state_dict(torch.load(model_path))
        self.optimizer = optim.Adam(self.model.parameters(), lr=0.001)
        self.bridge = Bridge()

    def run_inference_server(self):
        print(f"--- AI Brain Inference Server Started on {self.device} ---")
        while True:
            topic, data = self.bridge.receive(timeout=100)
            if topic == "search_state":
                # 1. Convert board state to a real 18x8x8 tensor
                state_tensor = board_to_tensor(data).to(self.device)
                
                # 2. Run Neural Network
                with torch.no_grad():
                    policy, value = self.model(state_tensor)
                
                # 3. Broadcast Evaluation back to C#
                # Policy: Flattened move probabilities, Value: win probability
                eval_payload = {
                    "hash": data["hash"],
                    "priors": policy.cpu().numpy().flatten().tolist()[:256], # Truncated for demo
                    "value": float(value.cpu().item())
                }
                self.bridge.send("evaluation_result", eval_payload)
                print(f"Evaluated state {data['hash']:X} | Value: {eval_payload['value']:.4f}")

    def self_play(self, num_games=10):
        for game_idx in range(num_games):
            board = chess.Board()
            game_history = []
            
            while not board.is_game_over():
                # Perform MCTS to find the best move
                move = self.mcts_search(board)
                game_history.append((board.copy(), move))
                board.push(move)
                
            # Update model based on game outcome
            result = board.result()
            self.train_on_game(game_history, result)
            print(f"Game {game_idx} finished with result: {result}")

    def mcts_search(self, board, iterations=800):
        # Skeleton MCTS search
        moves = list(board.legal_moves)
        return np.random.choice(moves)

    def train_on_game(self, history, result):
        pass

    def save_model(self, path="models/chess_net.pth"):
        torch.save(self.model.state_dict(), path)

if __name__ == "__main__":
    trainer = Trainer()
    trainer.run_inference_server()
