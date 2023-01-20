using DG.Tweening;
using Etienne;
using UnityEngine;
using UnityEngine.UI;

namespace Magaa
{
    public class Player : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float maxHealth = 100;
        [SerializeField, ReadOnly] private float currentHealth;
        [SerializeField] private Slider healthBar;
        [SerializeField, ReadOnly] private int currentAmmo;
        [SerializeField] private MagazineDisplay magazineDisplay;

        [Header("Weapon")]
        [SerializeField] private WeaponData startingWeapon;
        [SerializeField] private GameObject startingWeaponHolsterEmpty, startingWeaponHolsterFull;
        [SerializeField, ReadOnly] private Weapon currentWeapon;
        [SerializeField] private Transform weaponRoot;

        [Header("Hit")]
        [SerializeField, ReadOnly] private Material playerMaterial;
        [SerializeField] private float hitDuration;
        [SerializeField, ColorUsage(false, true)] private Color hitColor = Color.white;
        private Tween hitTween;
        private bool isShooting = false;
        private float attackSpeedMult = 1f;
        private float fireTimer;
        private bool isMoving;
        private Animator animator;

        private void Awake()
        {
            InputHandler.Instance.OnDirectionChanged += SetDirection;
            GameManager.Instance.SetPlayer(this);
            animator = GetComponentInChildren<Animator>();
        }

        private void Start()
        {
            SetHealth(maxHealth);
            SetWeapon(startingWeapon);
            TORENAMESHITNAME();
        }

        private void SetWeapon(WeaponData weapon)
        {
            bool isStartingWeapon = weapon == startingWeapon;
            startingWeaponHolsterEmpty.SetActive(isStartingWeapon);
            startingWeaponHolsterFull.SetActive(!isStartingWeapon);
            currentWeapon = GameObject.Instantiate(weapon.Prefab, weaponRoot);
            magazineDisplay.SetMaxAmmo(currentWeapon.Data.MagazineCapacity, currentWeapon.Data.AmmoMesh);
            currentAmmo = currentWeapon.Data.MagazineCapacity;
            isShooting = true; fireTimer = 0;
            currentWeapon.StartShooting(attackSpeedMult);
            float reloadingSpeed = currentWeapon.Data.ReloadSpeed;
            animator.SetFloat("ReloadingSpeed", reloadingSpeed);
        }

        private void Update()
        {
            if (isShooting)
            {
                fireTimer += Time.deltaTime;
                float fireDuration = currentWeapon.AttackDuration;
                if (fireTimer >= fireDuration)
                {
                    fireTimer -= fireDuration;
                    PlayShootAnimation();
                }
            }
            if (isMoving) Move();
        }

        private void Move()
        {
            transform.position += Time.deltaTime * walkSpeed * transform.forward;
        }

        private void SetDirection(Vector2 uiDirection)
        {
            bool wasMoving = isMoving;
            isMoving = uiDirection.sqrMagnitude > .1f;
            if (wasMoving != isMoving) animator.Play(isMoving ? "Walk" : "Idle", 0);
            if (!isMoving) return;

            transform.forward = GameManager.Instance.RoundDirectionFromUIDirection(uiDirection);
            healthBar.transform.parent.rotation = Quaternion.Euler(0, 45, 0);
        }

        private void PlayShootAnimation()
        {
            currentAmmo--;
            if (currentAmmo < 0)
            {
                Reload();
                return;
            }
            currentWeapon.Data.ShootCue.Play(transform.position);
            animator.Play("Shoot", 1);
            magazineDisplay.Shoot();
        }

        public void AnimationEndedReload()
        {
            magazineDisplay.Reload();
            currentWeapon.Data.MagInCue.Play(transform.position);
            currentAmmo = currentWeapon.Data.MagazineCapacity;
            isShooting = true;
            fireTimer = 0;
            currentWeapon.StartShooting(attackSpeedMult);
        }

        private void Reload()
        {
            animator.Play("Reload", 1);
            currentWeapon.Data.MagOutCue.Play(transform.position);
            isShooting = false;
            currentWeapon.StopShooting();
        }

        public void TORENAMESHITNAME()
        {
            GameManager.Instance.UnPauseGame();
            attackSpeedMult += .1f;
            currentWeapon.StartShooting(attackSpeedMult);
            float shootingAnimationSpeed = 1 / currentWeapon.AttackDuration;
            animator.SetFloat("ShootingSpeed", Mathf.Max(1f, shootingAnimationSpeed));
        }

        internal void Hit(float damage)
        {
            hitTween?.Complete();
            hitTween = DOTween.To(() => playerMaterial.GetColor("_EmissionColor"), x => playerMaterial.SetColor("_EmissionColor", x), hitColor, hitDuration).SetLoops(2, LoopType.Yoyo);
            Vibration.Vibrate(Mathf.RoundToInt(1000*hitDuration));
            SetHealth(currentHealth - damage);
        }

        private void SetHealth(float value)
        {
            currentHealth = value;
            healthBar.value = currentHealth / maxHealth;
            if (currentHealth <= 0) Die();
        }

        private void Die()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }
}
