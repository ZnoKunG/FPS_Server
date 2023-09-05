using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FPS_Server
{
    public class Player
    {
        public int Id;
        public string Username;
        public Vector3 Position;
        public Quaternion Rotation;
        public Lobby currentLobby;

        public Player(int id, string username, Vector3 position)
        {
            Id = id;
            Username = username;
            Position = position;
            Rotation = Quaternion.Identity;
        }

        public void JoinLobby(Lobby lobby)
        {
            currentLobby = lobby;
        }
    }
}
