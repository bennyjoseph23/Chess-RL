import streamlit as st
import os
import sys

# Ensure the project root is in the python path so it can find the 'ai' folder
sys.path.append(os.path.abspath(os.path.join(os.path.dirname(__file__), "..")))

import chess
import chess.svg
import base64
from ai.bridge import Bridge
import time
from streamlit_echarts import st_echarts
import numpy as np

st.set_page_config(layout="wide", page_title="Chess-RL Dashboard")

# Initialize Bridge as a subscriber only (listening to Engine and Brain)
if "bridge" not in st.session_state:
    st.session_state.bridge = Bridge(sub_addresses=["tcp://localhost:5555", "tcp://localhost:5556"])

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
    st.image(f"data:image/svg+xml;base64,{b64}")

def poll_bridge():
    """Polls ZeroMQ for all pending updates to keep the UI responsive."""
    any_updates = False
    while True:
        topic, data = st.session_state.bridge.receive(timeout=0)
        if not topic:
            break
        
        any_updates = True
        if topic == "game_update":
            full_move = data.get("FullMove", 1)
            # Use FEN for absolute synchronization
            fen = data.get("Fen")
            if fen:
                st.session_state.board.set_fen(fen)
            
            st.session_state.eval_data["full_move"] = full_move
            last_move_uci = data.get("LastMove", "None")
            st.session_state.eval_data["last_move"] = last_move_uci
            st.session_state.eval_data["turn"] = data.get("Side", "White")
            return True
            
        elif topic == "search_state":
            # Search intensity update
            if "Visits" in data:
                 st.session_state.eval_data["visit_counts"] = {"Simulations": data["Visits"]}
                 
        elif topic == "evaluation_result":
            # Real AI win probability update
            st.session_state.eval_data["value"] = data.get("value", 0.0)
            
    return any_updates

# UI Layout
st.title("♟️ Chess-RL: AlphaZero Simulation")

# Use containers to prevent layout jumping
board_container = st.container()
stats_container = st.container()

with board_container:
    col1, col2 = st.columns([1, 1])
    with col1:
        st.subheader(f"Current Turn: {st.session_state.eval_data['turn']}")
        render_board()
        if st.button("Reset Game", key="reset_btn"):
            st.session_state.board.reset()
            st.session_state.eval_data["last_move"] = "None"
            st.rerun()

    with col2:
        st.header("Brain Visualizer")
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

with stats_container:
    m1, m2 = st.columns(2)
    m1.metric("Last Move", st.session_state.eval_data["last_move"])
    m2.metric("Win Probability", f"{val:.2f}")
    
    st.subheader("MCTS Search Intensity")
    if st.session_state.eval_data["visit_counts"]:
        st.bar_chart(st.session_state.eval_data["visit_counts"])
    else:
        st.info("Waiting for engine search data...")

# Continuous Refresh Logic
# Use a checkbox at the bottom to control refresh
live_refresh = st.sidebar.checkbox("Live Refresh", value=True)

if live_refresh:
    if poll_bridge():
        st.rerun()
    time.sleep(0.05)
    st.rerun()
