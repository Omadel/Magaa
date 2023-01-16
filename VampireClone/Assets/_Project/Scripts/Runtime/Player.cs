using Etienne;
using System;
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
        [SerializeField, ReadOnly] private WeaponData currentWeapon;
        [SerializeField] private Transform weaponRoot;

        private Transform bulletSpawningTransform;
        private float fireTimer;
        private bool isMoving;

        private void Awake()
        {
            InputHandler.Instance.OnDirectionChanged += SetDirection;
            GameManager.Instance.SetPlayer(this);
        }

        private void Start()
        {
            SetHealth(maxHealth);
            SetWeapon(startingWeapon);
        }

        private void SetWeapon(WeaponData startingWeapon)
        {
            currentWeapon = startingWeapon;
            Transform t = GameObject.Instantiate(currentWeapon.Prefab, weaponRoot).transform;
            bulletSpawningTransform = t;
        }

        private void Update()
        {
            fireTimer += Time.deltaTime;
            float fireDuration = 1 / currentWeapon.FireRate;
            if (fireTimer >= fireDuration)
            {
                fireTimer -= fireDuration;
                Shoot();
            }
            if (isMoving) Move();
        }

        private void Move()
        {
            transform.position += Time.deltaTime * walkSpeed * transform.forward;
        }

        private void SetDirection(Vector2 uiDirection)
        {
            isMoving = uiDirection.sqrMagnitude > .1f;
            if (!isMoving) return;

            transform.forward = GameManager.Instance.RoundDirectionFromUIDirection(uiDirection);
            healthBar.transform.parent.rotation = Quaternion.Euler(0, 45, 0);
        }

        private void Shoot()
        {
            Bullet bullet = GameObject.Instantiate(currentWeapon.BulletPrefab);
            bullet.transform.position = bulletSpawningTransform.position;
            Vector3 direction = transform.forward;
            bullet.Shoot(direction, currentWeapon.BulletSpeed, currentWeapon.BulletDamage);
        }

        public void DoubleAttackSpeed()
        {
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
