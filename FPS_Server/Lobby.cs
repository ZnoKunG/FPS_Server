using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPS_Server
{
    public class Lobby
    {
        public string Name;
        public string Code;
        public int MaxPlayers;

        public int CurrentPlayerCount => inLobbyClients.Count;
        public Dictionary<int, Client> inLobbyClients;
        public Client HostClient;
        public bool IsReadyToStart => CurrentPlayerCount == MaxPlayers;

        public Lobby(string name, string code, Client _host, int maxPlayers = 2)
        {
            Name = name;
            Code = code;
            HostClient = _host;
            MaxPlayers = maxPlayers;
            inLobbyClients = new Dictionary<int, Client>();
        }


        public bool TryToJoin(int clientId)
        {
            if (CurrentPlayerCount == MaxPlayers) return false;
            if (inLobbyClients.ContainsKey(clientId)) return false;

            inLobbyClients.Add(clientId, Server.clients[clientId]);
            Server.clients[clientId].m_Player.JoinLobby(this);
            return true;
        }

        private void AbandonLobby()
        {
            foreach (Client client in inLobbyClients.Values)
            {
                Console.WriteLine($"{client.m_Player.Username} was force to left the lobby");
                ServerSend.ForceQuitLobby(client.Id);
            }

            Server.openingLobbies.Remove(this);
        }

        public void QuitLobby(int quitClientId)
        {
            Server.clients[quitClientId].m_Player.currentLobby = null;

            if (quitClientId == HostClient.Id)
            {
                AbandonLobby();
            }
            else
            {
                LeaveLobby(quitClientId);
            }
        }

        private void LeaveLobby(int quitClientId)
        {
            if (!Server.clients.ContainsKey(quitClientId)) return;

            inLobbyClients.Remove(quitClientId);
            ServerSend.QuitLobbySuccess(quitClientId, this);

            Console.WriteLine($"{Server.clients[quitClientId].m_Player.Username} has left the lobby");
            foreach (Client client in inLobbyClients.Values)
            {
                ServerSend.UpdateLobby(client.Id, quitClientId, Server.clients[quitClientId].m_Player.Username, false);
            }
        }
    }
}
