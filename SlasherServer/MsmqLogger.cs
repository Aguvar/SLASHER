using SlasherServer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace SlasherServer
{
    public class MsmqLogger : ISlasherLogger
    {

        private const string queuePath = @".\private$\logQueue";//Hacer que la direccion sea remota


        public MsmqLogger()
        {
                if (!MessageQueue.Exists(queuePath))
                {
                    MessageQueue.Create(queuePath);
                }
        }

        public void FinishMatchLog()
        {
            using (var sendQueue = new MessageQueue(queuePath))
            {
                var message = new Message(string.Empty)
                {
                    Label = "endlog"
                };

                sendQueue.Send(message);
            }
        }

        public void LogMatchLine(string line)
        {

            string messageContent = $"{line} - {DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}";

            using (var sendQueue = new MessageQueue(queuePath))
            {
                var message = new Message(messageContent)
                {
                    Label = "log"
                };

                sendQueue.Send(message);
            }
        }

        public void LogPlayerMatchResult(string nickname, char team, bool wonTheMatch)
        {
            string messageContent = $"{nickname}#{team}#{wonTheMatch.ToString()}";

            using (var sendQueue = new MessageQueue(queuePath))
            {
                var message = new Message(messageContent)
                {
                    Label = "statistic"
                };

                sendQueue.Send(message);
            }
        }

        public void LogPlayerScore(string nickname, int score, char team)
        {
            string messageContent = $"{nickname}#{score}#{team}";

            using (var sendQueue = new MessageQueue(queuePath))
            {
                var message = new Message(messageContent)
                {
                    Label = "highscore"
                };

                sendQueue.Send(message);
            }
        }
    }
}
