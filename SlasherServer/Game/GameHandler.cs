using SlasherServer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlasherServer.Game
{
    public class GameHandler
    {
        private Player[,] gameBoard;
        private List<Player> playerList;


        public GameHandler()
        {
            gameBoard = new Player[8,8];
            playerList = new List<Player>();
        }
    }
}
