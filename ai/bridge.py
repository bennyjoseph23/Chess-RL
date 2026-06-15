import zmq
import json

class Bridge:
    def __init__(self, sub_addresses=["tcp://localhost:5555"], pub_address=None):
        self.context = zmq.Context()
        
        # Subscriber for receiving data
        self.socket_sub = self.context.socket(zmq.SUB)
        for addr in sub_addresses:
            self.socket_sub.connect(addr)
        self.socket_sub.setsockopt_string(zmq.SUBSCRIBE, "")
        
        # Publisher is optional (only used by the Brain/Trainer)
        self.socket_pub = None
        if pub_address:
            self.socket_pub = self.context.socket(zmq.PUB)
            self.socket_pub.bind(pub_address)

    def receive(self, timeout=None):
        try:
            if timeout is not None:
                if self.socket_sub.poll(timeout) == 0:
                    return None, None
            
            topic = self.socket_sub.recv_string()
            data = self.socket_sub.recv_json()
            return topic, data
        except zmq.ZMQError:
            return None, None

    def send(self, topic, data):
        if self.socket_pub:
            self.socket_pub.send_string(topic, zmq.SNDMORE)
            self.socket_pub.send_json(data)
        else:
            print("Warning: Attempted to send on a subscriber-only bridge.")

    def close(self):
        self.socket_sub.close()
        if self.socket_pub:
            self.socket_pub.close()
        self.context.term()
