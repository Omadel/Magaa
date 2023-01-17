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

        [Header("Weapon")]
        [SerializeField] private WeaponData startingWeapon;
        [SerializeField, ReadOnly] private Weapon currentWeapon;
        [SerializeField] private Transform weaponRoot;
        [SerializeField] private AnimationClip shootingClip;
        private float attackSpeedMult = 1f;
        private Transform bulletSpawningTransform;
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
            currentWeapon = GameObject.Instantiate(weapon.Prefab, weaponRoot);
            bulletSpawningTransform = currentWeapon.transform;
        }

        private void Update()
        {
            fireTimer += Time.deltaTime;
            float fireDuration = currentWeapon.AttackDuration;
            if (fireTimer >= fireDuration)
            {
                fireTimer -= fireDuration;
                PlayShootAnimation();
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
            animator.Play("Shoot", 1);
        }

        public void TORENAMESHITNAME()
        {
            GameManager.Instance.UnPauseGame();
            attackSpeedMult += .1f;
            currentWeapon.StartShooting(attackSpeedMult);
            float shootingAnimationSpeed = 1 / currentWeapon.AttackDuration;
            Debug.Log(shootingAnimationSpeed);
            animator.SetFloat("ShootingSpeed", Mathf.Max(1f, shootingAnimationSpeed));
        }

        internal void Hit(float damage)
        {
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
