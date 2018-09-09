using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlasherServer.Interfaces
{
    interface Player
    {
        int Id { get; set; }
        int Health { get; set; }

        void Attack();

    }
}
