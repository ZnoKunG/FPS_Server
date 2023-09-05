using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FPS_Server
{
    public class Client
    {
        public static int dataBufferSize = 4096;

        public int Id;
        public TCP Tcp;
        public Player m_Player;

        public Client(int _id)
        {
            Id = _id;
            Tcp = new TCP(Id);
        }

        public void CreatePlayerData(string playerName)
        {
            m_Player = new Player(Id, playerName);
        }

        public void SendIntoGame()
        {
            // Spawn other clients on client's game
            foreach (Client connectedClient in Server.clients.Values)
            {
                if (connectedClient.Id == Id) continue;
                if (connectedClient.m_Player == null) continue;

                ServerSend.SpawnPlayer(Id, connectedClient.m_Player);
            }

            // Spawn this client gameObject to existing clients (including itself)
            foreach (Client connectedClient in Server.clients.Values)
            {
                if (connectedClient.m_Player == null) continue;

                ServerSend.SpawnPlayer(connectedClient.Id, m_Player);
            }
        }

        public void Disconnect()
        {
            Console.WriteLine($"{m_Player.Username} has disconnected.");

            if (m_Player.currentLobby != null)
            {
                m_Player.currentLobby.QuitLobby(Id);
            }

            m_Player = null;
            Tcp.Disconnect();
        }


        public class TCP
        {
            public TcpClient Socket { get; private set; }

            private readonly int id;
            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;

            public TCP(int _id)
            {
                id = _id;
            }

            public void Connect(TcpClient _socket)
            {
                Socket = _socket;
                Socket.ReceiveBufferSize = dataBufferSize;
                Socket.SendBufferSize = dataBufferSize;

                stream = Socket.GetStream();

                receiveBuffer = new byte[dataBufferSize];

                // Wait to read Client Message ...
                Console.WriteLine($"Client from IP {Socket.Client.RemoteEndPoint} is connected. Waiting for the message...");

                receivedData = new Packet();
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

                ServerSend.Welcome(id, "Welcome to the server!");
            }

            public void SendData(Packet packet)
            {
                try
                {
                    if (Socket == null)
                    {
                        Console.WriteLine("Socket is null");
                        return;
                    }

                    stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error sending the data: {e}");
                }
            }

            private bool HandleData(byte[] data)
            {
                int packetLength = 0;

                receivedData.SetBytes(data);

                if (receivedData.UnreadLength() >= 4)
                {
                    packetLength = receivedData.ReadInt();

                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }

                while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
                {
                    byte[] packetBytes = receivedData.ReadBytes(packetLength);
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet packet = new Packet(packetBytes))
                        {
                            int packetId = packet.ReadInt();
                            Server.packetHandlers[packetId](id, packet);
                        }
                    });

                    packetLength = 0;

                    if (receivedData.UnreadLength() >= 4)
                    {
                        packetLength = receivedData.ReadInt();

                        if (packetLength <= 0)
                        {
                            return true;
                        }
                    }
                }

                // Still have some bytes waiting to be sent
                if (packetLength <= 1)
                {
                    return true;
                }

                return false;
            }

            private void ReceiveCallback(IAsyncResult _result)
            {
                try
                {
                    int byteLength = stream.EndRead(_result);

                    if (byteLength <= 0)
                    {
                        Server.clients[id].Disconnect();
                        return;
                    }

                    byte[] data = new byte[byteLength];
                    Array.Copy(receiveBuffer, data, byteLength);

                    receivedData.Reset(HandleData(data));

                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error receiving TCP data: {e}");
                    Server.clients[id].Disconnect();
                }
            }

            public void Disconnect()
            {
                Socket.Close();
                stream = null;
                receivedData = null;
                receiveBuffer = null;
                Socket = null;
            }

        }


    }
}
