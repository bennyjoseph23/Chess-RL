 # Chess-RL
    2
    3 A high-performance, polyglot Reinforcement Learning chess AI inspired by AlphaZero.
    4
    5 ## 🏗️ Architecture
    6 - **Engine:** C# (.NET 9) with Magic Bitboards & Zobrist Hashing.
    7 - **AI Brain:** Python (PyTorch) Residual CNN.
    8 - **Communication:** ZeroMQ (Async Pub/Sub).
    9 - **Interface:** Streamlit Interactive Dashboard.
   10
   11 ## Getting Started
   12 1. **Install Dependencies:** `pip install -r requirements.txt`
   13 2. **Build Engine:** `dotnet build`
   14 3. **Run Brain:** `python -c "from ai.training import Trainer; Trainer().run_inference_server()"`
   15 4. **Run UI:** `streamlit run ui/dashboard.py`
   16 5. **Run Simulator:** `dotnet run --project ChessRL.Simulator`
   17
   18 ## Features
   19 - O(1) Move Generation via Magic Bitboards.
   20 - MCTS with PUCT formula for optimal exploration.
   21 - 18-Plane Feature Engineering for Neural Network input.
   22 - Real-time visualization of Win Probability and Search Intensity.
