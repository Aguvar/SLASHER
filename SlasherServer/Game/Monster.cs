using SlasherServer.Interfaces;
using System;

namespace SlasherServer.Game
{
    public class Monster : IPlayer
    {
        private Guid _id;
        public Guid Id { get => _id; set => _id = value; }

        public string Nickname { get; set; }

        private int _health;
        public int Health { get => _health; set => _health = Math.Max(0, value); }

        private int _attackDamage = 10;
        public int AttackDamage { get { return _attackDamage; } private set { _attackDamage = value; } }

        public int Score { get; set; }

        public Monster()
        {
            Score = 0;
            Health = 100;
        }

        public char GetPlayerType()
        {
            return 'M';
        }

        public override bool Equals(object player)
        {
            if (player is IPlayer)
            {
                return Id.Equals(((IPlayer)player).Id);
            }
            else
            {
                return false;
            }
        }

        public void ReceiveDamageFrom(IPlayer attacker)
        {
            Health -= attacker.AttackDamage;
        }
    }
}
