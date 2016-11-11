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
        public byte[] recvBuf;
        public int writeIndex;
    }

    volatile SocketStruct _socketStruct;
    State _oldstate = State.DisConnected; //只由Update修改和更新
    

    public Network()
    {
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
        _socketStruct.state = State.Connecting;
        _socketStruct.recvBuf = new byte[Config.maxPackSize * 2];
        _socketStruct.writeIndex = 0;
        new Thread(() => StartConnect(_socketStruct)).Start();
    }

    void StartConnect(SocketStruct socketStruct)
    {
        try
        {
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
            socketStruct.socket.Blocking = false;
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
                //Receive
                int count = _socketStruct.socket.Receive(_socketStruct.recvBuf, _socketStruct.writeIndex, _socketStruct.recvBuf.Length - _socketStruct.writeIndex, SocketFlags.None);
                if (count > 0)
                {
                    _socketStruct.writeIndex += count;

                    int readIndex = 0;

                    while (_socketStruct.writeIndex - readIndex >= Config.packSizeLength)
                    {
                        UInt32 length = BitConverter.ToUInt32(_socketStruct.recvBuf, readIndex);
                        if (length > Config.maxPackSize)
                        {
                            throw new Exception("Pack out of size.");
                        }

                        if (_socketStruct.writeIndex - readIndex >= length + Config.packSizeLength)
                        {
                            //TODO::readIndex += Config.packSizeLength，读出length长的字节，这是Pack，解析并分发执行完毕
                            readIndex += (int)length + Config.packSizeLength;
                        }
                        else
                        {
                            break;
                        }
                    }


                    int reCount = _socketStruct.writeIndex - readIndex;
                    Buffer.BlockCopy(_socketStruct.recvBuf, readIndex, _socketStruct.recvBuf, 0, reCount);
                    _socketStruct.writeIndex = reCount;
                }

                //Send
                //需要发送队列，类型由Protobuf决定，转化为byte[]并推入socket
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
