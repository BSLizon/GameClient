using System;
using System.Net;
using System.Net.Sockets;

public class Network
{
    public static Network getInstance()
    {
        return Singleton<Network>.getInstance();
    }

    public enum State
    {
        DisConnected,
        Connecting,
        Connected
    }

    Socket _socket;
    State _state = State.DisConnected;
    public State state
    {
        get
        {
            return _state;
        }
    }

    public Network()
    {
        Reconnect();
    }

    public void Reconnect()
    {
        _state = State.DisConnected;
        if (_socket != null)
        {
            _socket.BeginDisconnect(false, new AsyncCallback(DisconnectCallback), _socket);
            _socket = null;
        }

        IPAddress ipAddress = IPAddress.Parse(Config.serverIP);
        IPEndPoint remoteEP = new IPEndPoint(ipAddress, Config.serverPort);
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _socket.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), _socket);
        _state = State.Connecting;
    }

    void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndConnect(ar);
            _state = State.Connected;
            Log.Info("Connect Complete: " + _socket.RemoteEndPoint.ToString());
        }
        catch (Exception e)
        {
            _state = State.DisConnected;
            Log.Info(e.ToString());
        }
    }

    void DisconnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndDisconnect(ar);
            Log.Info("Disconnect Complete: " + _socket.RemoteEndPoint.ToString());
        }
        catch (Exception e)
        {
            Log.Info(e.ToString());
        }
    }
}
