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

    public class Event_RecvMessage : Event
    {
        public byte[] data;
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
        public Queue<byte[]> sendDataQ;
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
        _socketStruct.recvBuf = new byte[Config.maxInboundPackSize * 2];
        _socketStruct.writeIndex = 0;
        _socketStruct.sendDataQ = new Queue<byte[]>();
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
                if (_socketStruct.socket.Available > 0)
                {
                    int count = _socketStruct.socket.Receive(_socketStruct.recvBuf, _socketStruct.writeIndex, _socketStruct.recvBuf.Length - _socketStruct.writeIndex, SocketFlags.None);
                    if (count > 0)
                    {
                        _socketStruct.writeIndex += count;

                        int readIndex = 0;

                        while (_socketStruct.writeIndex - readIndex >= Config.packSizeLength)
                        {
                            UInt32 length = (UInt32)System.Net.IPAddress.NetworkToHostOrder((int)BitConverter.ToUInt32(_socketStruct.recvBuf, readIndex));
                            if (length > Config.maxInboundPackSize || length <= 0)
                            {
                                throw new Exception("wrong pack size.");
                            }

                            if (_socketStruct.writeIndex - readIndex >= length + Config.packSizeLength)
                            {
                                byte[] data = new byte[length];
                                Buffer.BlockCopy(_socketStruct.recvBuf, readIndex + Config.packSizeLength, data, 0, (int)length);
                                EventBus.Notify(new Event_RecvMessage { data = data });

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
                }

                while (_socketStruct.sendDataQ.Count > 0)
                {
                    byte[] orgData = _socketStruct.sendDataQ.Dequeue();
                    if (orgData.Length == 0)
                    {
                        Log.Warn("pack size zero");
                        continue;
                    }
                    byte[] sendData = new byte[Config.packSizeLength + orgData.Length];
                    var test = System.Net.IPAddress.HostToNetworkOrder(orgData.Length);
                    Buffer.BlockCopy(BitConverter.GetBytes((UInt32)System.Net.IPAddress.HostToNetworkOrder(orgData.Length)), 0, sendData, 0, Config.packSizeLength);
                    Buffer.BlockCopy(orgData, 0, sendData, Config.packSizeLength, orgData.Length);
                    _socketStruct.socket.BeginSend(sendData, 0, sendData.Length, SocketFlags.None, null, null);
                }
            }
            catch (Exception e)
            {
                _socketStruct.state = State.DisConnected;
                Log.Error("Handle pack error: " + e.ToString());
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

    public void Send(byte[] data)
    {
        _socketStruct.sendDataQ.Enqueue(data);
    }
}
