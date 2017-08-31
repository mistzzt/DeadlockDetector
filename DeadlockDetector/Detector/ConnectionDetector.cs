using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Terraria.Net;
using Terraria.Net.Sockets;
using TShockAPI;

namespace DeadlockDetector.Detector
{
    public class ConnectionDetector : IDetector
    {
        private ISocket _socket;

        private readonly byte[] _readBuffer = new byte[1024];
        private readonly byte[] _writeBuffer = new byte[131070];
        private readonly BinaryWriter _writer;

        private bool _working;
        private bool _status;

        private readonly int _versionStrLength;

#if CHECKRESPONSE
        private readonly byte[] _connectResponse = {0x04, 0x00, 0x03, 0x00};
#endif

        public ConnectionDetector()
        {
            var writerStream = new MemoryStream(_writeBuffer);
            _writer = new BinaryWriter(writerStream);

            _versionStrLength = WriteVersion();
        }

        public bool Detect()
        {
            try
            {
                _socket = CreateSocket();

                _working = true;
                Log("start working");

                _socket.Connect(new TcpAddress(IPAddress.Loopback, TShock.Config.ServerPort));

                if (_socket.IsConnected())
                {
                    Log("connected");
                }
                else
                {
                    throw new SocketException(ConnectionRefusedCode);
                }
            }
            catch (SocketException ex)
            {
                if (ex.ErrorCode == ConnectionRefusedCode)
                {
                    Log("connection failed");
                }

                Log(ex.ToString());

                CloseSocket();

                return true;
            }

            try
            {
                _socket.AsyncSend(_writeBuffer, 0, _versionStrLength, SendCallback);
                _socket.AsyncReceive(_readBuffer, 0, _readBuffer.Length, ReceiveCallback);

                for (var i = 0; i < 3; i++)
                {
                    if (!_working)
                    {
                        break;
                    }

                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Log("Unhandled exception in socket send & receive");
                Log(ex.ToString());
            }
            finally
            {
                CloseSocket();
            }

            return _working || _status;
        }

        private void ReceiveCallback(object state, int size)
        {
            Log("received");

            _status = false;
#if CHECKRESPONSE
            for (var i = 0; i < _connectResponse.Length; i++)
            {
                if (_connectResponse[i] == _readBuffer[i])
                    continue;

                _status = true;
                break;
            }
#endif

            _working = false;
            Log("working stopped");
        }

        private void SendCallback(object state)
        {
            Log("sent");
        }

        private void CloseSocket()
        {
            _socket.Close();
            _socket = null;

            Log("Socket closed");
        }

        private int WriteVersion()
        {
            _writer.BaseStream.Position = 0L;
            var position = _writer.BaseStream.Position;
            _writer.BaseStream.Position += 2L;
            _writer.Write((byte) 1);
            _writer.Write("Terraria" + 194);
            var end = (int) _writer.BaseStream.Position;
            _writer.BaseStream.Position = position;
            _writer.Write((short) end);
            _writer.BaseStream.Position = end;

            return end;
        }

        public string Name => GetType().Name;

        /*[System.Diagnostics.Conditional("DEBUG")]*/
        private static void Log(string format, params object[] args)
        {
            TShock.Log.ConsoleInfo(format, args);
        }

        private static ISocket CreateSocket() => new TcpSocket();

        private const int ConnectionRefusedCode = 0x274D;
    }
}