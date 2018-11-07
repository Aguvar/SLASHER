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
                            AddLog(message);
                            break;
                        case "endLog":
                            FinishGameLog();
                            break;
                        case "highscore":
                            AddScore(messageArray);
                            break;
                        case "statistic":
                            LogPlayerMatchStatistics(messageArray);
                            break;
                    }
                }
            }
        }

        private static void AddLog(Message message)
        {
            GameLog gameLog = GameLog.GetInstance();

            gameLog.AddEntry(message.Body.ToString());
        }

        private static void FinishGameLog()
        {
            GameLog gameLog = GameLog.GetInstance();

            gameLog.FinishLog();
        }

        private static void LogPlayerMatchStatistics(string[] messageArray)
        {
            StatisticsHandler statisticsHandler = StatisticsHandler.GetInstance();

            string nickname = messageArray[0];
            char team = messageArray[1][0];
            bool wonTheMatch = bool.Parse(messageArray[2]);
            statisticsHandler.AddPlayerMatchResult(nickname, team, wonTheMatch);
        }

        private static void AddScore(string[] messageArray)
        {
            HighScoreHandler highScoreHandler = HighScoreHandler.GetInstance();

            string nickname = messageArray[0];
            int score = int.Parse(messageArray[1]);
            char team = messageArray[2][0];
            highScoreHandler.AddScore(nickname, score, team);
        }
    }
}