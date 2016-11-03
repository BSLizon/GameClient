using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

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

    Queue<byte[]> sendQueue;
    Queue<byte[]> recvQueue;

    public Network()
    {
        sendQueue = new Queue<byte[]>();
        recvQueue = new Queue<byte[]>();
        Reconnect();
    }

    public void Reconnect()
    {
        try
        {
            _state = State.DisConnected;
            if (_socket != null)
            {
                _socket.BeginDisconnect(false, new AsyncCallback(DisconnectCallback), _socket);
            }
        }
        catch (Exception e)
        {
            Log.Warn(e.ToString());
        }

        try
        {
            IPAddress ipAddress = IPAddress.Parse(Config.serverIP);
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, Config.serverPort);
            _state = State.Connecting;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), _socket);
        }
        catch (Exception e)
        {
            _state = State.DisConnected;
            Log.Warn(e.ToString());
        }
    }

    void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndConnect(ar);
            socket.SendTimeout = Config.socketTimeout;
            socket.NoDelay = true;
            _state = State.Connected;
            Log.Info("Connect Complete: " + _socket.RemoteEndPoint.ToString());
        }
        catch (Exception e)
        {
            _state = State.DisConnected;
            Log.Warn(e.ToString());
        }
    }

    void DisconnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndDisconnect(ar);
            Log.Info("Disconnect Complete");
        }
        catch (Exception e)
        {
            Log.Warn(e.ToString());
        }
    }

    void Update()
    {
        if (_state == State.Connected)
        {
            //TODO::到底同步解包分发还是异步解包分发
        }
        else if (_state == State.DisConnected)
        {
            Reconnect();
        }
        else if (_state == State.Connecting)
        {
            Log.Debug("Network Connecting: " + _socket.RemoteEndPoint.ToString());
        }
        else
        {
            Log.Warn("Unknown state: " + _state);
        }
    }
}
