using Etienne;
using UnityEngine;

namespace Magaa
{
    [CreateAssetMenu(menuName = "Magaa/Weapon")]
    public class WeaponData : ScriptableObject
    {
        public float FireRate => fireRate;
        public float BulletSpeed => bulletSpeed;
        public int BulletDamage => bulletDamage;
        public float ReloadSpeed => 1 / (reloadingDuration / reloadClip.length);
        public int MagazineCapacity => magazineCapacity;
        public Mesh AmmoMesh => ammoMesh;
        public Weapon Prefab => prefab;
        public Cue ShootCue  => shootCue;
        public Cue ReloadCue => reloadCue;
        public Cue MagInCue => magInCue;
        public Cue MagOutCue => magOutCue;

        [SerializeField] private float fireRate = .5f;
        [SerializeField] private float bulletSpeed = 12f;
        [SerializeField] private int bulletDamage = 50;
        [SerializeField] private int magazineCapacity = 12;
        [SerializeField] private Mesh ammoMesh;
        [SerializeField] private Weapon prefab;
        [SerializeField] private float reloadingDuration = 1f;
        [SerializeField] private AnimationClip reloadClip;
        [Header("Audio")]
        [SerializeField] Cue shootCue;
        [SerializeField] Cue reloadCue;
        [SerializeField] Cue magInCue;
        [SerializeField] Cue magOutCue;
    }
}
