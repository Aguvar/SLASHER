using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LogServer
{
    public class GameLog
    {
        private static  GameLog _instance;

        private List<string> logEntries;

        private GameLog()
        {
            logEntries = new List<string>();

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
            string ret = "";

            foreach (var entry in logEntries)
            {
                ret +=  "\n" + entry;
            }

            return ret;
        }

        
    }
}