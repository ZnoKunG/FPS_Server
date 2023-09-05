using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FPS_Server
{
    public class ServerHandle
    {
        public static void WelcomeReceived(int fromClient, Packet packet)
        {
            int clientId = packet.ReadInt();
            string clientUsername = packet.ReadString();

            Console.WriteLine($"{Server.clients[clientId].Tcp.Socket.Client.RemoteEndPoint} connected successfully and is now player {fromClient} with name {clientUsername}.");

            if (fromClient != clientId)
            {
                Console.WriteLine($"Player {clientUsername} with ID {fromClient} has assumed the wrong client ID {clientId}!");
            }

            Server.clients[fromClient].CreatePlayerData(clientUsername);
        }

        #region Lobby
        public static void CreateLobby(int fromClient, Packet packet)
        {
            int clientId = packet.ReadInt();
            string lobbyName = packet.ReadString();

            for (int i = 0; i < Server.openingLobbies.Count; i++)
            {
                if (Server.openingLobbies[i].Name == lobbyName)
                {
                    Console.WriteLine($"Create lobby name {lobbyName} failed!");
                    ServerSend.CreateLobbySuccess(clientId, false, null);
                    return;
                }
            }

            string lobbyCode = RandomString(5);
            Console.WriteLine($"Client {clientId} created lobby name {lobbyName} with code {lobbyCode}");

            // Create Lobby and Initialize Host member
            Lobby lobby = new Lobby(lobbyName, lobbyCode, Server.clients[clientId]);
            bool canJoin = lobby.TryToJoin(clientId);

            if (!canJoin)
            {
                Console.WriteLine("Something is wrong with the lobby logic or maxPlayers = 0");
                return;
            }

            Server.openingLobbies.Add(lobby);

            ServerSend.CreateLobbySuccess(clientId, true, lobby);
        }

        public static void RequestToJoinLobby(int fromClient, Packet packet)
        {
            int joinClientId = packet.ReadInt();
            string lobbyCode = packet.ReadString();

            Console.WriteLine($"Client {joinClientId} try to join lobby {lobbyCode}...");

            foreach (Lobby lobby in Server.openingLobbies)
            {
                if (lobbyCode != lobby.Code) continue;

                bool canJoin = lobby.TryToJoin(joinClientId);
                ServerSend.JoinLobbySuccess(joinClientId, canJoin, lobby);
                Console.WriteLine($"{Server.clients[joinClientId].m_Player.Username} join lobby: {canJoin}");

                if (!canJoin) return;

                foreach (Client lobbyClient in lobby.inLobbyClients.Values)
                {
                    if (lobbyClient.Id == joinClientId) continue;

                    ServerSend.UpdateLobby(lobbyClient.Id, joinClientId, Server.clients[joinClientId].m_Player.Username, true);
                    Console.WriteLine($"Send update lobby to client {lobbyClient.m_Player.Username}");
                }

                break;
            }
        }

        public static void QuitLobby(int fromClient, Packet packet)
        {
            int quitClientId = packet.ReadInt();

            Lobby currentClientLobby = Server.clients[quitClientId].m_Player.currentLobby;

            if (currentClientLobby == null)
            {
                Console.WriteLine("Client is not currently in any lobby");
                return;
            }

            currentClientLobby.QuitLobby(quitClientId);
        }
        #endregion

        #region Gameplay
        public static void StartGameRequest(int fromClient, Packet packet)
        {
            int startClientId = packet.ReadInt();

            Lobby gameLobby = Server.clients[startClientId].m_Player.currentLobby;
            if (!gameLobby.IsReadyToStart)
            {
                Console.WriteLine($"Lobby {gameLobby.Name} is not ready yet.");
            }

            Console.WriteLine($"Start Game in lobby {gameLobby.Name}");
            foreach (Client client in gameLobby.inLobbyClients.Values)
            {
                ServerSend.StartGame(client.Id);
            }
        }

        public static void SpawnPlayersInLobby(int fromClient, Packet packet)
        {
            int clientId = packet.ReadInt();

            Lobby gameLobby = Server.clients[clientId].m_Player.currentLobby;

            Console.WriteLine($"Spawn players in lobby to Client {clientId}");
            // Spawn other clients on client's game
            foreach (Client connectedClient in gameLobby.inLobbyClients.Values)
            {
                if (connectedClient.m_Player == null) continue;

                ServerSend.SpawnPlayer(fromClient, connectedClient.m_Player, connectedClient.m_Player.Position, connectedClient.m_Player.Rotation);
            }
        }
        #endregion

        public static string RandomString(int length)
        {
            Random rand = new Random();

            // String of alphabets 
            String str = "abcdefghijklmnopqrstuvwxyz";

            // Initializing the empty string
            String ran = "";

            for (int i = 0; i < length; i++)
            {

                // Selecting a index randomly
                int x = rand.Next(26);

                // Appending the character at the 
                // index to the random string.
                ran = ran + str[x];
            }

            return ran;
        }
    }
}
