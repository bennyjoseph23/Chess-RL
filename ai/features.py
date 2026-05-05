import numpy as np
import torch

def board_to_tensor(board_data):
    """
    Converts a board state into an 18x8x8 tensor representation.
    Planes:
    0-5:   White pieces (P, N, B, R, Q, K)
    6-11:  Black pieces (P, N, B, R, Q, K)
    12:    Side to move (all 1s if White, all 0s if Black)
    13-16: Castling rights (WK, WQ, BK, BQ)
    17:    En Passant square
    """
    # Initialize 18 planes of 8x8
    planes = np.zeros((18, 8, 8), dtype=np.float32)
    
    # In a real scenario, board_data would contain the Bitboards
    # For this implementation, we expect the bitboards for each piece/color
    # piece_bbs: list of 12 bitboards (6 white, 6 black)
    
    if "bitboards" in board_data:
        for i in range(12):
            bb = board_data["bitboards"][i]
            planes[i] = bitboard_to_numpy(bb)
            
    # Metadata planes
    if board_data.get("side_to_move") == 0: # White
        planes[12] = 1.0
        
    # Castling (4 bits)
    rights = board_data.get("castling_rights", 0)
    if rights & 1: planes[13] = 1.0 # WK
    if rights & 2: planes[14] = 1.0 # WQ
    if rights & 4: planes[15] = 1.0 # BK
    if rights & 8: planes[16] = 1.0 # BQ
    
    return torch.from_numpy(planes).unsqueeze(0) # Add batch dimension

def bitboard_to_numpy(bb):
    """Converts a 64-bit integer bitboard into an 8x8 numpy array."""
    arr = np.zeros(64, dtype=np.float32)
    for i in range(64):
        if (bb >> i) & 1:
            arr[i] = 1.0
    return arr.reshape((8, 8))
