using System;

namespace SlasherServer.Interfaces
{
    public interface IPlayer
    {
        Guid Id { get; set; }
        string Nickname { get; set; }
        int Health { get; set; }
        int AttackDamage { get; }
        int Score { get; set; }
        
        void ReceiveDamageFrom(IPlayer attacker);
        char GetPlayerType();
    }
}
