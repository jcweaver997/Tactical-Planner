using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Tactical_Provider
{
    class TacticalCom
    {
        public enum State
        {
            Client, Server
        }
        public State state { get; set; }

        private const int defaultPort = 5505;
        public int port { get; set; }

        private const string defaultIP = "104.236.94.98";
        public string IP { get; set; }

        public const byte GETFIVE = 2;
        public const byte GETSPECIFIC = 3;
        public const byte CLOSE = 4;
        public const byte DATA = 1;

        private TcpListener listener;
        public TcpClient client;
        private TcpClient lastClient;

        public TacticalCom(State state)
        {
            this.state = state;
            port = defaultPort;
            IP = defaultIP;
        }

        public void Start()
        {
            switch (state)
            {
                case State.Client:
                    StartClient();
                    break;
                case State.Server:
                    StartServer();
                    break;
                default:
                    Console.WriteLine("invalid tactical com state");
                    break;
            }
        }
        private void StartServer()
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Listen();
        }



        private void StartClient()
        {
            client = new TcpClient(IP, port);
            client.ReceiveBufferSize = 1000000;
        }

        private void Listen()
        {
            listener.BeginAcceptTcpClient(ListenCallback, listener);
        }

        public delegate void ClientAdded(TcpClient client);
        public ClientAdded onClientAdded { get; set; }
        private void ListenCallback(IAsyncResult ar)
        {
            try
            {
                TcpClient client = listener.EndAcceptTcpClient(ar);
                client.SendBufferSize = 1000000;
                lastClient = client;
                if (onClientAdded != null)
                {
                    onClientAdded(client);
                }

                Listen();
            }
            catch (Exception)
            {
                Console.WriteLine("Error accepting client");
            }

        }

        public void Send(byte[] b)
        {
            client.GetStream().Write(b, 0, b.Length);
            client.GetStream().Flush();
        }
        public void Send(byte b)
        {
            client.GetStream().WriteByte(b);
            client.GetStream().Flush();
            if (b == CLOSE)
            {
                client.GetStream().Close();
                client.Close();
            }
        }
        public void SendTo(byte[] b, TcpClient client)
        {
            client.GetStream().Write(b, 0, b.Length);
            client.GetStream().Flush();


        }
        public void SendTo(byte b, TcpClient client)
        {
            client.GetStream().WriteByte(b);
            client.GetStream().Flush();
            if (b == CLOSE)
            {
                client.GetStream().Close();
                client.Close();
            }

        }
        public byte[] Recv(int size = 128)
        {
            byte[] buffer = new byte[size];
            while (client.Available < size)
            {
                System.Threading.Thread.Sleep(10);
            }
            client.GetStream().Read(buffer, 0, size);
            if (buffer[0] == CLOSE && size == 1)
            {
                client.GetStream().Close();
                client.Close();
            }
            return buffer;

        }
        public byte[] RecvFrom(TcpClient client, int size = 2)
        {
            while (client.Available < size)
            {
                System.Threading.Thread.Sleep(10);
            }
            byte[] buffer = new byte[size];
            try
            {
                client.GetStream().Read(buffer, 0, size);
            }
            catch (Exception)
            {
                Console.WriteLine("Error reading from client");
            }

            return buffer;
        }
        public void Close()
        {
            if (client != null)
            {
                client.Close();
            }
            if (listener != null)
            {
                listener.Stop();
            }
            if (lastClient != null)
            {
                lastClient.Close();
            }
        }


    }
}
