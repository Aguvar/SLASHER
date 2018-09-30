using System;

namespace SlasherServer.Interfaces
{
    public interface IPlayer
    {
        Guid Id { get; set; }
        int Health { get; set; }
        int AttackDamage { get; }
        
        
        //Observer pattern can be implemented
        void ReceiveDamageFrom(IPlayer attacker);
        char GetPlayerType();
    }
}
