using Etienne;
using UnityEngine;
using UnityEngine.Rendering;

namespace Magaa
{
    [CreateAssetMenu(menuName = "Magaa/Weapon")]
    public class WeaponData : ScriptableObject
    {
        public float FireRate => fireRate;
        public float BulletSpeed => bulletSpeed;
        public float BulletDamage => bulletDamage;
        public float ReloadingDuration => reloadingDuration;
        public int MagazineCapacity => magazineCapacity;
        public Mesh AmmoMesh => ammoMesh;
        public Weapon Prefab => prefab;
        public float ReloadDuration=>reloadClip.length;

        [SerializeField] private float fireRate = .5f;
        [SerializeField] private float bulletSpeed = 12f;
        [SerializeField] private float bulletDamage = 50f;
        [SerializeField] float reloadingDuration = 1f;
        [SerializeField] int magazineCapacity = 12;
        [SerializeField] Mesh ammoMesh;
        [SerializeField] private Weapon prefab;
        [SerializeField] AnimationClip reloadClip;
    }
}
