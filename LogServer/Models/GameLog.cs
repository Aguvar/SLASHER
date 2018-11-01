using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LogServer
{
    public class GameLog
    {
        private static  GameLog _instance;

        private string _latestLog; 
        private List<string> _logEntries;

        private GameLog()
        {
            _logEntries = new List<string>();
            _latestLog = "Nothing ATM";
        }

        public static GameLog GetInstance()
        {
            if (_instance == null)
            {
                _instance = new GameLog();
            }

            return _instance;
        }

        public string GetLog()
        {
            return _latestLog;
        }

        public void AddEntry(string entry)
        {
            _logEntries.Add(entry);
        }

        public void FinishLog()
        {
            string newLog = "";

            foreach (var entry in _logEntries)
            {
                newLog += "\n" + entry;
            }

            _latestLog = newLog;

            _logEntries.Clear();
        }
    }
}