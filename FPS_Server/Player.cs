using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FPS_Server
{
    public class Player
    {
        public int Id;
        public string Username;

        public int Score;
        public Lobby currentLobby;

        public Player(int id, string username, int score = 0)
        {
            Id = id;
            Username = username;
            Score = score;
        }

        public void JoinLobby(Lobby lobby)
        {
            currentLobby = lobby;
        }
    }
}
