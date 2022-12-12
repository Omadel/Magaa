using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VampireClone
{
    public interface IDamageable 
    {
        int Health { get; }

        void Hit(int value);
        void Heal(int value);
        void Die();
    }
    public interface IAttacker 
    {
        int Damage { get; }

        void Attack();
    }
}
