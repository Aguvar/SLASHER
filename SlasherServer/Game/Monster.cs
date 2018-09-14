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
        public int Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int Health { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        
        
        public Monster()
        {
            this.Health = 100;
        }
        
        public int AttackDamage()
        {
            return 10;
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
            return 'M';
        }
    }
}
