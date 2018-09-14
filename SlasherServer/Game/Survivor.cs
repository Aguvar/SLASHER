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
        public int Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int Health { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Survivor()
        {
            this.Health = 20;
        }
        
        public int AttackDamage()
        {
            return 5;
        }
        
        public bool ReceiveDamage(int damage)
        {
            this.Health -= damage;
            if(this.Health <= 0)
            {
                return true;
            } else
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
