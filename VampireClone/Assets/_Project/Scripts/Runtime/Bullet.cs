using UnityEngine;

namespace Magaa
{
    public class Bullet : MonoBehaviour
    {
        public int Damage => damage;
        public void SetDamage(int damage) => this.damage = damage;
        private int damage;
    }
}
