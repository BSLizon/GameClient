using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;

public class Network
{
    public static Network getInstance()
    {
        return Singleton<Network>.getInstance();
    }

    public class Event_StateChange : Event
    {
        public State state;
    }


    public enum State
    {
        None,
        DisConnected,
        Connecting,
        Connected
    }

    class SocketStruct//单独写成结构，是为了多线程下一口气替换socket和state
    {
        public Socket socket;
        public State state = State.DisConnected;
    }

    volatile SocketStruct _socketStruct;
    State _oldstate = State.DisConnected; //只由Update修改和更新

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
        Log.Info("Socket Start Reconnect");
        try
        {
            if (_socketStruct != null)
            {
                _socketStruct.state = State.DisConnected;
                if (_socketStruct.socket != null)
                {
                    _socketStruct.socket.Close(Config.socketCloseTimeout);
                    Log.Info("Old Socket Close Complete");
                }
            }
        }
        catch (Exception e)
        {
            Log.Warn(e.ToString());
        }

        _socketStruct = new SocketStruct();
        _socketStruct.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        new Thread(() => StartConnect(_socketStruct)).Start();
    }

    void StartConnect(SocketStruct socketStruct)
    {
        try
        {
            socketStruct.state = State.Connecting;
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(Config.serverIP), Config.serverPort);
            Log.Info("Socket Connecting: " + remoteEP.ToString());
            IAsyncResult result = socketStruct.socket.BeginConnect(remoteEP, null, null);
            if (!result.AsyncWaitHandle.WaitOne(Config.socketConnectTimeout, true))
            {
                socketStruct.socket.Close();
                throw new TimeoutException();
            }

            if (!socketStruct.Equals(_socketStruct))
            {
                socketStruct.socket.Close();
                throw new Exception("Close Old Socket.");
            }

            socketStruct.socket.EndConnect(result);
            socketStruct.socket.SendTimeout = Config.socketSendTimeout;
            socketStruct.socket.NoDelay = true;
            socketStruct.state = State.Connected;
            Log.Info("Socket Connect Complete: " + socketStruct.socket.RemoteEndPoint.ToString());
        }
        catch (Exception e)
        {
            socketStruct.state = State.DisConnected;
            Log.Warn(e.ToString());
        }
    }

    public void Update()
    {
        if (_socketStruct.state == State.Connected)
        {
            if (_oldstate != State.Connected)
            {
                _oldstate = _socketStruct.state;
                EventBus.Notify(new Event_StateChange() { state = State.Connected });
            }

            try
            {
                //TODO::同步解包分发
            }
            catch (Exception e)
            {
                _socketStruct.state = State.DisConnected;
                Log.Error("handle pack error: " + e.ToString());
            }
        }
        else if (_socketStruct.state == State.DisConnected)
        {
            if (_oldstate != State.DisConnected)
            {
                _oldstate = _socketStruct.state;
                EventBus.Notify(new Event_StateChange() { state = State.DisConnected });
            }

            Reconnect();
        }
        else if (_socketStruct.state == State.Connecting)
        {
            Log.Debug("Socket Connecting...");
        }
        else
        {
            Log.Warn("Unhandle state: " + _socketStruct.state);
        }
    }
}
