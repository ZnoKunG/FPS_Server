using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace FPS_Server
{
    public static class Server
    {
        public static int MaxPlayers { get; private set; }
        public static int Port { get; private set; }

        private static TcpListener tcpListener;

        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
        
        // receive packets
        public delegate void PacketHandler(int fromClient, Packet packet);
        public static Dictionary<int, PacketHandler> packetHandlers;

        public static List<Lobby> openingLobbies = new List<Lobby>();

        public static void Start(int _maxPlayers, int _port)
        {
            MaxPlayers = _maxPlayers;
            Port = _port;

            Console.WriteLine("Starting Server ...");
            InitializeServerData();

            tcpListener = new TcpListener(IPAddress.Any, Port);
            Console.WriteLine($"Server started on PORT {Port}");
            tcpListener.Start();

            Console.WriteLine("Start accepting clients to connect ...");
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

        }

        private static void TCPConnectCallback(IAsyncResult _result)
        {
            TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
            Console.WriteLine($"Incoming connection from IP: {_client.Client.RemoteEndPoint}");

            for (int i = 1; i <= MaxPlayers; i++)
            {
                // Has not intialize Tcp socket
                if (clients[i].Tcp.Socket == null)
                {
                    clients[i].Tcp.Connect(_client);
                    return;
                }
            }

            Console.WriteLine("The server is full! Try increasing maxPlayers in Server initialization");
        }

        private static void InitializeServerData()
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                clients.Add(i, new Client(i));
            }

            Console.WriteLine("Initialized Server Data");

            packetHandlers = new Dictionary<int, PacketHandler>
            {
                { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
                { (int)ClientPackets.createLobby, ServerHandle.CreateLobby },
                { (int)ClientPackets.joinLobby, ServerHandle.RequestToJoinLobby },
                { (int)ClientPackets.quitLobby, ServerHandle.QuitLobby },
                { (int)ClientPackets.startGameRequest, ServerHandle.StartGameRequest },
                { (int)ClientPackets.spawnPlayersInLobby, ServerHandle.SpawnPlayersInLobby },
            };
        }

        public static void SendData(int toClient, Packet packet)
        {
            clients[toClient].Tcp.SendData(packet);
        }
    }

}
