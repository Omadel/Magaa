using UnityEngine;

namespace Magaa
{
    public class Bullet : MonoBehaviour
    {
        public float Damage => damage;
        public void SetDamage(float damage) => this.damage = damage;
        private float damage;
    }
}
