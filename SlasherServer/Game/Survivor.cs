using SlasherServer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlasherServer.Game
{
    class Survivor : IPlayer
    {
        public Guid Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        private int _health;
        public int Health { get => _health; set => _health = value; }

        public int AttackDamage => 5;

        public Survivor()
        {
            this.Health = 20;
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
            return 'S';
        }
    }
}
