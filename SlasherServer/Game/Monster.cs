using SlasherServer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlasherServer.Game
{
    public class Monster : IPlayer
    {
        private Guid _id;
        public Guid Id { get => _id; set => _id = value; }

        private int _health = 100;
        public int Health { get => _health; set => _health = value; }

        private int _attackDamage = 10;
        public int AttackDamage { get { return _attackDamage; } private set { _attackDamage = value; } }

        public Monster()
        {
            this.Health = 100;
        }

        public bool ReceiveDamage(int damage)
        {
            this.Health -= damage;
            if (this.Health <= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public char Type()
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
    }
}
