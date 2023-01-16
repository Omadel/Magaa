using UnityEngine;

namespace Magaa
{
    [CreateAssetMenu(menuName = "Magaa/Weapon")]
    public class WeaponData : ScriptableObject
    {
        public float FireRate => fireRate;
        public float BulletSpeed => bulletSpeed;
        public float BulletDamage => bulletDamage;
        public Bullet BulletPrefab => bulletPrefab;
        public Transform Prefab => prefab;

        [SerializeField] private float fireRate = .5f;
        [SerializeField] private float bulletSpeed = 10f;
        [SerializeField] private float bulletDamage = 50f;
        [SerializeField] private Bullet bulletPrefab;
        [SerializeField] private Transform prefab;
    }
}
