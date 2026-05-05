import streamlit as st
import chess
import chess.svg
import base64
from ai.bridge import Bridge
import time
from streamlit_echarts import st_echarts
import numpy as np

st.set_page_config(layout="wide", page_title="Chess-RL Dashboard")

# Initialize Bridge as a singleton in session state
if "bridge" not in st.session_state:
    st.session_state.bridge = Bridge()

if "board" not in st.session_state:
    st.session_state.board = chess.Board()

if "eval_data" not in st.session_state:
    st.session_state.eval_data = {
        "policy": {}, 
        "visit_counts": {}, 
        "value": 0.0,
        "last_move": "None",
        "turn": "White"
    }

def render_board():
    # Highlight last move if available
    last_move = None
    try:
        if st.session_state.eval_data["last_move"] != "None":
            last_move = chess.Move.from_uci(st.session_state.eval_data["last_move"])
    except:
        pass
        
    svg = chess.svg.board(st.session_state.board, lastmove=last_move, size=450)
    b64 = base64.b64encode(svg.encode('utf-8')).decode('utf-8')
    st.image(f"data:image/svg+xml;base64,{b64}", use_column_width=False)

def poll_bridge():
    """Polls ZeroMQ for any updates from the C# Engine."""
    topic, data = st.session_state.bridge.receive(timeout=10)
    if not topic:
        return False
        
    if topic == "game_update":
        st.session_state.eval_data["last_move"] = data.get("LastMove", "None")
        st.session_state.eval_data["turn"] = data.get("Side", "White")
        # In a real app, you'd sync the FEN here
        # st.session_state.board.set_fen(data.get("Fen"))
        if data.get("LastMove") != "None":
            try:
                move = chess.Move.from_uci(data["LastMove"])
                if move in st.session_state.board.legal_moves:
                    st.session_state.board.push(move)
            except:
                pass
        return True
        
    if topic == "search_state":
        st.session_state.eval_data["value"] = data.get("Value", 0.0)
        # Random data simulation for visit counts if not provided
        if "Visits" in data:
             st.session_state.eval_data["visit_counts"] = {"Simulations": data["Visits"]}
        return True
    return False

# UI Layout
st.title("♟️ Chess-RL: AlphaZero Simulation")

col1, col2 = st.columns([1, 1])

with col1:
    st.subheader(f"Current Turn: {st.session_state.eval_data['turn']}")
    render_board()
    
    if st.button("Reset Game"):
        st.session_state.board.reset()
        st.session_state.eval_data["last_move"] = "None"
        st.rerun()

with col2:
    st.header("Brain Visualizer")
    
    # Value Gauge
    val = st.session_state.eval_data["value"]
    option = {
        "series": [{
            "type": 'gauge',
            "startAngle": 180,
            "endAngle": 0,
            "min": -1,
            "max": 1,
            "splitNumber": 10,
            "axisLine": { "lineStyle": { "width": 10, "color": [[0.3, '#ff4b4b'], [0.7, '#f1c40f'], [1, '#2ecc71']] } },
            "pointer": { "width": 5 },
            "detail": { "formatter": '{value}', "fontSize": 30 },
            "data": [{ "value": round(val, 3), "name": 'Win Prob' }]
        }]
    }
    st_echarts(options=option, height="350px")
    
    # Metrics
    m1, m2 = st.columns(2)
    m1.metric("Last Move", st.session_state.eval_data["last_move"])
    m2.metric("Win Probability", f"{val:.2f}")

    # MCTS Visits
    st.subheader("MCTS Search Intensity")
    if st.session_state.eval_data["visit_counts"]:
        st.bar_chart(st.session_state.eval_data["visit_counts"])
    else:
        st.info("Waiting for engine search data...")

# Continuous Refresh Loop
if st.checkbox("Live Refresh", value=True):
    if poll_bridge():
        st.rerun()
    time.sleep(0.1)
    st.rerun()
