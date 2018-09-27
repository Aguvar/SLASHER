using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlasherServer.Interfaces
{
    public interface IPlayer
    {
        Guid Id { get; set; }
        int Health { get; set; }
        int AttackDamage { get; }
        
        
        //Observer pattern can be implemented
        bool ReceiveDamage(int damage);
        char Type();
    }
}
