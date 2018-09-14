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
        private IPlayer[,] gameBoard;
        private List<IPlayer> players;

        private const int BOARD_SIZE = 8;
        private const int MAX_CAPACITY = 15;


        public GameHandler()
        {
            gameBoard = new IPlayer[BOARD_SIZE, BOARD_SIZE];
            players = new List<IPlayer>();
        }

        public void Attack(IPlayer playerWhoAttacks)
        {
            Position playerPosition = GetPlayerPosition(playerWhoAttacks);
            if (playerPosition != null)
            {
                AttackNeighbours(playerPosition, playerWhoAttacks.AttackDamage());
            }
        }
        
        public bool IsGameFull()
        {
            return players.Count <= MAX_CAPACITY;
        }
        
        public void AddPlayer(IPlayer player)
        {
            Position position = GetRandomUnoccupiedPosition();
            gameBoard[position.Row, position.Col] = player;
            players.Add(player);
        }

        private Position GetRandomUnoccupiedPosition()
        {
            Random r = new Random();
            int row = r.Next(0, BOARD_SIZE - 1);
            int col = r.Next(0, BOARD_SIZE - 1);
            while(gameBoard[row, col] != null)
            {
                row = r.Next(0, BOARD_SIZE - 1);
                col = r.Next(0, BOARD_SIZE - 1);                
            }
            return new Position(row, col);
        }

        private void AttackNeighbours(Position playerPosition, int damage)
        {
            int minRow = Math.Max(0, playerPosition.Row - 1);
            int maxRow = Math.Min(gameBoard.Length, playerPosition.Row + 1);
            int minCol = Math.Max(0, playerPosition.Col - 1);
            int maxCol = Math.Min(gameBoard.Length, playerPosition.Col + 1);

            for (int row = minRow; row < maxRow; row++)
            {
                for (int col = minCol; row < maxCol; col++)
                {
                    IPlayer neighbour = gameBoard[row, col];
                    if(neighbour != null)
                    {
                        bool isKilled = neighbour.ReceiveDamage(damage);
                        if(isKilled)
                        {
                            //Delete player and disconnect client
                        }
                    }
                }
            }
        }

        private Position GetPlayerPosition(IPlayer player)
        {
            for (int row = 0; row < gameBoard.Length; row++)
            {
                for (int col = 0; row < gameBoard.Length; col++)
                {
                    if (gameBoard[row, col].Equals(player))
                    {
                        return new Position(row, col);
                    }
                }
            }
            return null;
        }

        private string PrintPlayerSurroundings(Position position, int maxDistance)
        {
            string output = "";
            int minRow = Math.Max(0, position.Row - maxDistance);
            int maxRow = Math.Min(gameBoard.Length, position.Row + maxDistance);
            int minCol = Math.Max(0, position.Col - maxDistance);
            int maxCol = Math.Min(gameBoard.Length, position.Col + maxDistance);

            for (int row = minRow; row < maxRow; row++)
            {
                for (int col = minCol; row < maxCol; col++)
                {
                    IPlayer player = gameBoard[row, col];
                    if (row == position.Row && col == position.Col)
                    {
                        output += " *";
                    }
                    else if (player == null)
                    {
                        output += " -";
                    }
                    else
                    {
                        output += " " + player.GetType();
                    }
                }
                output += "\n";
            }
            return output;
        }
    }
}
