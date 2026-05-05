import zmq
import json

class Bridge:
    def send(self, topic, data):
        self.socket_pub.send_string(topic, zmq.SNDMORE)
        self.socket_pub.send_json(data)

    def __init__(self, sub_address="tcp://localhost:5555", pub_address="tcp://*:5556"):
        self.context = zmq.Context()
        # Subscriber for receiving states from C#
        self.socket_sub = self.context.socket(zmq.SUB)
        self.socket_sub.connect(sub_address)
        self.socket_sub.setsockopt_string(zmq.SUBSCRIBE, "")
        
        # Publisher for sending evaluations back to C#
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

    def close(self):
        self.socket_sub.close()
        self.socket_pub.close()
        self.context.term()
