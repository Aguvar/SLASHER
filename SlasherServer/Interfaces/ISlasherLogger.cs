using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlasherServer.Interfaces
{
    public interface ISlasherLogger
    {
        void LogMatchLine(string line);
        void FinishMatchLog();
        void LogPlayerScore(string nickname, int score, char team);
        void LogPlayerMatchResult(string nickname, char team, bool wonTheMatch);
    }
}
