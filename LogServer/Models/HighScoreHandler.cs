using System.Collections.Generic;
using System.Linq;

namespace LogServer.Models
{
    public class HighScoreHandler
    {
        private static HighScoreHandler _instance;

        private List<HighScore> _highScores;

        private HighScoreHandler()
        {
            _highScores = new List<HighScore>();
        }

        public static HighScoreHandler GetInstance()
        {
            if (_instance == null)
            {
                _instance = new HighScoreHandler();
            }

            return _instance;
        }

        private class HighScore
        {
            public string Nickname { get; private set; }
            public int Score { get; private set; }

            public HighScore(string nickname, int score)
            {
                Nickname = nickname;
                Score = score;
            }

            public override string ToString()
            {
                return $"{Nickname} - {Score} Points";
            }
        }

        public string GetHighScores()
        {
            if (_highScores.Count == 0)
            {
                return "No highscores yet";
            }

            string ret = "";

            foreach (var score in _highScores)
            {
                ret += $"{1 + _highScores.IndexOf(score)}) {score}\n";
            }

            return ret;
        }

        public void AddScore(string nickname, int score)
        {
            HighScore highScore = new HighScore(nickname, score);

            _highScores.Add(highScore);
            _highScores.OrderByDescending(h => h.Score);

            if (_highScores.Count > 10)
            {
                _highScores.RemoveAt(10);
            }
        }
    }
}