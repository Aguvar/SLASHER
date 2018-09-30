using SlasherServer.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SlasherServer.Game
{
    public class GameHandler
    {
        private IPlayer[,] gameBoard;
        private List<IPlayer> players;

        private const int BOARD_SIZE = 8;
        private const int MAX_CAPACITY = 15;

        private Stopwatch matchTimer;

        public bool MatchOngoing { get; private set; }

        public string Result { get; internal set; }

        public GameHandler()
        {
            matchTimer = new Stopwatch();
            gameBoard = new IPlayer[BOARD_SIZE, BOARD_SIZE];
            players = new List<IPlayer>();
        }

        public void ExecutePlayerAttack(IPlayer playerWhoAttacks)
        {
            Position playerPosition = GetPlayerPosition(playerWhoAttacks);
            if (playerPosition != null)
            {
                for (int currentRow = playerPosition.Row - 1; currentRow <= playerPosition.Row + 1; currentRow++)
                {
                    for (int currentCol = playerPosition.Col - 1; currentCol <= playerPosition.Col + 1; currentCol++)
                    {
                        if (!PositionOutOfBounds(currentCol,currentRow) && gameBoard[currentRow,currentCol] != null && !(currentRow == playerPosition.Row && currentCol == playerPosition.Col))
                        {
                            IPlayer target = gameBoard[currentRow, currentCol];
                            target.ReceiveDamageFrom(playerWhoAttacks);
                        }
                    }
                }
            }
        }

        public bool Move(IPlayer player, Position deltaPosition)
        {
            Position currentPosition = GetPlayerPosition(player);
            Position nextPosition = new Position(currentPosition.Row + deltaPosition.Row, currentPosition.Col + deltaPosition.Col);
            if (nextPosition.Row >= 0 && nextPosition.Row < BOARD_SIZE && nextPosition.Col >= 0 && nextPosition.Col < BOARD_SIZE)
            {
                if (gameBoard[nextPosition.Row, nextPosition.Col] == null)
                {
                    gameBoard[currentPosition.Row, currentPosition.Col] = null;
                    gameBoard[nextPosition.Row, nextPosition.Col] = player;
                    return true;
                }
            }
            return false;
        }

        public bool IsGameFull()
        {
            return players.Count == MAX_CAPACITY;
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
            while (gameBoard[row, col] != null)
            {
                row = r.Next(0, BOARD_SIZE - 1);
                col = r.Next(0, BOARD_SIZE - 1);
            }
            return new Position(row, col);
        }

        public Position GetPlayerPosition(IPlayer player)
        {
            for (int row = 0; row < BOARD_SIZE; row++)
            {
                for (int col = 0; col < BOARD_SIZE; col++)
                {
                    if (gameBoard[row, col] != null && gameBoard[row, col].Equals(player))
                    {
                        return new Position(row, col);
                    }
                }
            }
            return null;
        }

        public string GetPlayerSurroundings(Position position, int maxDistance)
        {
            string output = "\n";

            for (int currentCol = position.Col - maxDistance; currentCol <= position.Col + maxDistance; currentCol++)
            {
                for (int currentRow = position.Row - maxDistance; currentRow <= position.Row + maxDistance; currentRow++)
                {
                    if (PositionOutOfBounds(currentCol, currentRow))
                    {
                        output += "X";
                    }
                    else
                    {
                        var currentPlayer = gameBoard[currentRow, currentCol];
                        if (currentRow == position.Row && currentCol == position.Col)
                        {
                            output += "*";
                        }
                        else if (currentPlayer == null)
                        {
                            output += "-";
                        }
                        else
                        {
                            output += currentPlayer.GetPlayerType();
                        }
                    }

                    output += " ";
                }
                output += "\n";
            }

            return output;
        }

        private static bool PositionOutOfBounds(int currentCol, int currentRow)
        {
            return currentRow < 0 || currentRow > BOARD_SIZE - 1 || currentCol < 0 || currentCol > BOARD_SIZE - 1;
        }

        internal void StartGame()
        {
            MatchOngoing = true;
            matchTimer.Start();
        }

        /// <summary>
        /// Cleans up the game and calculates who the winner is and makes all necessary information available on properties
        /// </summary>
        internal void EndGame()
        {
            MatchOngoing = false;
            matchTimer.Stop();
            matchTimer.Reset();


            //Put the winners where the winners should be

            players.Clear();
        }

        internal bool IsOver()
        {
            return false;
            long currentMatchTime = matchTimer.ElapsedMilliseconds;

            //180 seconds = 180000 milliseconds

            if (currentMatchTime > 180000)
            {
                return true;
            }
            else if (currentMatchTime > 60000)
            {
                return OnlySurvivorsAlive() || OnlyOneMonsterRemains();
            }
            return false;
        }

        private bool OnlyOneMonsterRemains()
        {
            bool monsterFound = false;

            for (int x = 0; x < BOARD_SIZE; x++)
            {
                for (int y = 0; y < BOARD_SIZE; y++)
                {
                    if (gameBoard[x, y] != null && gameBoard[x, y].Health > 0)
                    {
                        if (gameBoard[x, y].GetPlayerType() == 'S' || monsterFound)
                        {
                            return false;
                        }
                        else
                        {
                            monsterFound = true;
                        }
                    }
                }
            }

            return monsterFound;
        }

        private bool OnlySurvivorsAlive()
        {
            for (int x = 0; x < BOARD_SIZE; x++)
            {
                for (int y = 0; y < BOARD_SIZE; y++)
                {
                    if (gameBoard[x, y] != null && gameBoard[x, y].GetPlayerType() == 'M' && gameBoard[x, y].Health > 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal List<IPlayer> GetWinners()
        {
            throw new NotImplementedException();
        }

        public IPlayer GetPlayerById(Guid id)
        {
            return players.FirstOrDefault<IPlayer>(p => p.Id.Equals(id));
        }

    }
}
