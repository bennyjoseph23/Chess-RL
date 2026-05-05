using NetMQ;
using NetMQ.Sockets;
using System.Text.Json;

namespace ChessRL.Engine;

public class Messenger : IDisposable
{
    private readonly PublisherSocket _pubSocket;
    private readonly SubscriberSocket _subSocket;
    private readonly string _pubAddress;
    private readonly string _subAddress;

    public Messenger(string pubAddress = "tcp://*:5555", string subAddress = "tcp://localhost:5556")
    {
        _pubAddress = pubAddress;
        _subAddress = subAddress;
        
        _pubSocket = new PublisherSocket();
        _pubSocket.Bind(_pubAddress);

        _subSocket = new SubscriberSocket();
        _subSocket.Connect(_subAddress);
        _subSocket.Subscribe("");
    }

    public void BroadcastEvaluation(string topic, object data)
    {
        string json = JsonSerializer.Serialize(data);
        _pubSocket.SendMoreFrame(topic).SendFrame(json);
    }

    public string ReceiveEvaluation(int timeoutMs = 100)
    {
        if (_subSocket.TryReceiveFrameString(TimeSpan.FromMilliseconds(timeoutMs), out string topic))
        {
            return _subSocket.ReceiveFrameString();
        }
        return null;
    }

    public void Dispose()
    {
        _pubSocket?.Dispose();
        _subSocket?.Dispose();
        NetMQConfig.Cleanup();
    }
}
