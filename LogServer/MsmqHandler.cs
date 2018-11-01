using LogServer.Models;
using System;
using System.Messaging;

namespace LogServer
{
    internal class MsmqHandler
    {
        private const string queuePath = @".\private$\logQueue";

        public MsmqHandler()
        {
            if (!MessageQueue.Exists(queuePath))
            {
                MessageQueue.Create(queuePath);
            }

        }

        internal void HandleQueue()
        {
            GameLog gameLog = GameLog.GetInstance();

            while (true)
            {
                using (var queue = new MessageQueue(queuePath))
                {
                    queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });

                    var message = queue.Receive();
                    string[] messageArray = message.Body.ToString().Split('#');

                    switch (message.Label)
                    {
                        case "log":
                            gameLog.AddEntry(message.Body.ToString());
                            break;
                        case "endLog":
                            gameLog.FinishLog();
                            break;
                        case "highscore":
                            AddScore(messageArray);
                            break;
                        case "statistic":
                            LogPlayerMatch(messageArray);
                            break;

                    }
                }
            }
        }

        private static void LogPlayerMatch(string[] messageArray)
        {
            StatisticsHandler statisticsHandler = StatisticsHandler.GetInstance();

            string nickname = messageArray[0];
            char team = messageArray[1][0];
            char result = messageArray[2][0];
            statisticsHandler.AddPlayerMatchResult(nickname, team, result);
        }

        private static void AddScore(string[] messageArray)
        {
            HighScoreHandler highScoreHandler = HighScoreHandler.GetInstance();

            string nickname = messageArray[0];
            int score = int.Parse(messageArray[1]);
            highScoreHandler.AddScore(nickname, score);
        }
    }
}