using System;
using System.Collections.Generic;
using System.Linq;

namespace LogServer.Models
{
    public class StatisticsHandler
    {
        private static StatisticsHandler _instance;

        private Dictionary<string, List<MatchResult>> _statistics;

        private StatisticsHandler()
        {
            _statistics = new Dictionary<string, List<MatchResult>>();
        }

        public static StatisticsHandler GetInstance()
        {
            if (_instance == null)
            {
                _instance = new StatisticsHandler();
            }

            return _instance;
        }

        private class MatchResult
        {
            public char Team { get; private set; }
            public char Result { get; private set; }
            public DateTime Date { get; private set; }

            public MatchResult(char team, char result)
            {
                Team = team;
                Result = result;
                Date = DateTime.Now;
            }
        }

        public string GetStatistics()
        {
            if (_statistics.Count == 0)
            {
                return "There are no statistics yet";
            }

            string ret = "";

            foreach (var nickname in _statistics.Keys)
            {
                ret += GetPlayerStatistics(nickname) + "\n\n";
            }

            return ret;
        }

        public string GetPlayerStatistics(string nickname)
        {
            if (!_statistics.ContainsKey(nickname))
            {
                return $"There are no statistics for {nickname}";
            }

            var playerStatistics = _statistics[nickname];

            int survivorTimes = playerStatistics.Where(mr => mr.Team.Equals('S')).Count();
            int monsterTimes = playerStatistics.Where(mr => mr.Team.Equals('M')).Count();

            int wins = playerStatistics.Where(mr => mr.Result.Equals('W')).Count();
            int losses = playerStatistics.Where(mr => mr.Result.Equals('L')).Count();

            return $"Last 10 matches statistics for {nickname}\nWins: {wins}\nLosses: {losses}\nTimes played as Survivor: {survivorTimes}\nTimes played as Monster: {monsterTimes}";
        }

        public void AddPlayerMatchResult(string nickname, char team, char result)
        {
            if (!_statistics.ContainsKey(nickname))
            {
                _statistics[nickname] = new List<MatchResult>();
            }

            _statistics[nickname].Add(new MatchResult(team, result));

            _statistics[nickname].OrderByDescending(mr => mr.Date);

            if (_statistics[nickname].Count > 10)
            {
                _statistics[nickname].RemoveAt(10);
            }

        }
    }
}