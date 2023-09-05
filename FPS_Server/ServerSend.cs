using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FPS_Server
{
    public class ServerSend
    {
        private static void SendTCPDataToClient(int toClient, Packet packet)
        {
            packet.WriteLength();
            Server.SendData(toClient, packet);
        }

        private static void SendTCPDataToAll(Packet packet)
        {
            packet.WriteLength();

            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.SendData(i, packet);
            }
        }

        public static void Welcome(int toClient, string message)
        {
            using (Packet packet = new Packet((int)ServerPackets.welcome))
            {
                packet.Write(message);
                packet.Write(toClient);

                SendTCPDataToClient(toClient, packet);
            }

        }

        public static void SpawnPlayer(int toClient, Player player)
        {
            using (Packet packet = new Packet((int)ServerPackets.spawnPlayer))
            {
                packet.Write(player.Id);
                packet.Write(player.Username);
                packet.Write(player.Score);

                SendTCPDataToClient(toClient, packet);
            }
        }


        public static void CreateLobbySuccess(int toClient, bool isSuccess, Lobby lobby)
        {
            using (Packet packet = new Packet((int)ServerPackets.createLobbySuccess))
            {
                packet.Write(toClient);
                packet.Write(isSuccess);

                if (!isSuccess)
                {
                    SendTCPDataToClient(toClient, packet);
                    return;
                }

                packet.Write(lobby.Name);
                packet.Write(lobby.Code);
                packet.Write(lobby.MaxPlayers);
                packet.Write(lobby.CurrentPlayerCount);
                packet.Write(lobby.HostClient.m_Player.Username);

                SendTCPDataToClient(toClient, packet);
            }
        }

        public static void UpdateLobby(int toClient, int joinClientId, string joinClientName, bool isAddToLobby)
        {
            using (Packet packet = new Packet((int)ServerPackets.updateLobby))
            {
                packet.Write(toClient);
                packet.Write(joinClientId);
                packet.Write(joinClientName);
                packet.Write(isAddToLobby);

                SendTCPDataToClient(toClient, packet);
            }
        }

        public static void JoinLobbySuccess(int toClient, bool isSuccess, Lobby lobby)
        {
            using (Packet packet = new Packet((int)ServerPackets.joinLobbySuccess))
            {
                packet.Write(toClient);
                packet.Write(isSuccess);

                // Only send fail join lobby data
                if (!isSuccess)
                {
                    SendTCPDataToClient(toClient, packet);
                    return;
                }

                packet.Write(lobby.Name);
                packet.Write(lobby.Code);
                packet.Write(lobby.MaxPlayers);
                packet.Write(lobby.CurrentPlayerCount);

                foreach (Client client in lobby.inLobbyClients.Values)
                {
                    packet.Write(client.m_Player.Username);
                }

                SendTCPDataToClient(toClient, packet);
            }
        }

        public static void ForceQuitLobby(int toClient)
        {
            using (Packet packet = new Packet((int)ServerPackets.forceQuitLobby))
            {
                packet.Write(toClient);

                SendTCPDataToClient(toClient, packet);
            }
        }

        public static void QuitLobbySuccess(int toClient, Lobby lobby)
        {
            using (Packet packet = new Packet((int)ServerPackets.quitLobbySuccess))
            {
                packet.Write(toClient);
                packet.Write(lobby.Name);

                SendTCPDataToClient(toClient, packet);
            }
        }

        public static void StartGame(int toClient)
        {
            using (Packet packet = new Packet((int)ServerPackets.startGame))
            {
                packet.Write(toClient);

                SendTCPDataToClient(toClient, packet);
            }
        }
    }
}
