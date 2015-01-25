using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Combot.IRCServices.TCP
{
    class TCPInterface
    {
        internal event Action<TCPError> TCPErrorEvent;
        internal event Action<int> TCPConnectionEvent;
        internal bool Connected = false;

        private IPEndPoint _serverIP = null;
        private int _readTimeout = 250;
        private Socket _tcpClient;
        private NetworkStream _tcpStream;
        private int _allowedFailedCount;
        private int _currentFailedCount;

        internal TCPInterface()
        {
        }

        internal bool Connect(IPAddress IP, int port, int readTimeout, int allowedFailedCount = 0)
        {
            _serverIP = new IPEndPoint(IP, port);
            _readTimeout = readTimeout;
            _allowedFailedCount = allowedFailedCount;
            _currentFailedCount = 0;

            try
            {
                _tcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _tcpClient.Connect(_serverIP);
                _tcpClient.ReceiveTimeout = _readTimeout;

                _tcpStream = new NetworkStream(_tcpClient);
                Connected = true;
                return true;
            }
            catch
            {
                Action<TCPError> localEvent = TCPErrorEvent;
                if (localEvent != null)
                {
                    TCPError error = new TCPError();
                    error.Message = string.Format("Unable to connect to {0} on port {1}", _serverIP.Address, _serverIP.Port);
                    localEvent(error);
                }
            }
            return false;
        }

        internal void Disconnect()
        {
            Connected = false;
            if (_tcpStream != null)
            {
                _tcpStream.Close();
            }
            if (_tcpClient != null)
            {
                _tcpClient.Close();
            }
        }

        internal void Write(string data)
        {
            if (_tcpStream.CanWrite && Connected)
            {
                byte[] message = System.Text.Encoding.UTF8.GetBytes(data + Environment.NewLine);
                _tcpStream.Write(message, 0, message.Length);
            }
        }

        internal string Read()
        {
            try
            {
                if (_tcpStream.CanRead && Connected)
                {
                    byte[] readBytes = new byte[100000];
                    _tcpStream.Read(readBytes, 0, readBytes.Length);
                    string result = Encoding.UTF8.GetString(readBytes, 0, readBytes.Length);
                    // Reset Failed Counter
                    _currentFailedCount = 0;
                    return result.TrimEnd('\0');
                }
            }
            catch (IOException)
            {
                _currentFailedCount++;
                Action<TCPError> localEvent = TCPErrorEvent;
                if (localEvent != null && _tcpStream.CanRead)
                {
                    TCPError error = new TCPError();
                    error.Message = string.Format("Read Timeout, No Response from Server in {0}ms", _readTimeout);
                    localEvent(error);
                }
            }
            catch (Exception ex)
            {
                _currentFailedCount++;
                Action<TCPError> localEvent = TCPErrorEvent;
                if (localEvent != null)
                {
                    TCPError error = new TCPError();
                    error.Message = ex.Message;
                    localEvent(error);
                }
            }

            if (_currentFailedCount > _allowedFailedCount)
            {
                Action<int> localEvent = TCPConnectionEvent;
                if (localEvent != null)
                {
                    localEvent(_currentFailedCount);
                }
                Disconnect();
                _currentFailedCount = 0;
            }
            return null;
        }
    }
}
