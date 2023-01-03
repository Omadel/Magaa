using DG.Tweening;
using Etienne;
using Etienne.Pools;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace VampireClone
{
    [DefaultExecutionOrder(-1)]
    public class Player : Singleton<Player>, IDamageable, IAttacker
    {
        public int Health => health;
        public int Damage => damage;

        public Unit UNIT;

        [Header("Player")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private int health = 100;
        [SerializeField] private float shootCooldown = 1f;
        [SerializeField] private int damage = 10;
        [SerializeField] private float moveBufferDelay = .3f;
        [Header("Shoot")]
        [SerializeField] private ParticleSystem shootFXPrefab;
        [SerializeField] private Transform shootFXTransform;
        [SerializeField] private float durationFX = 3f;
        [SerializeField] private Bullet bulletPrefab;
        [SerializeField] private float bulletSpeed;
        [SerializeField] private ParticleSystem bloodFXPrefab;

        [Header("Cached fields")]
        [SerializeField, ReadOnly] private Animator animator;
        [SerializeField, ReadOnly] private Vector3 forward;
        [SerializeField, ReadOnly] private Vector3 right;

        private Vector2 inputDirection;
        private new Camera camera;
        private Timer shootTimer;
        private Sequence shootSequence;
        private ComponentPool<ParticleSystem> bloodFXPool, shootFXPool;
        private ComponentPool<Bullet> bulletQueue;

        private void Reset()
        {
            CacheAnimator();
            CalculateDirectionVectors();
        }

        private void OnValidate()
        {
            shootTimer?.SetDuration(shootCooldown);
            shootTimer?.Restart();
        }

        [ContextMenu(nameof(CalculateDirectionVectors))]
        private void CalculateDirectionVectors()
        {
            Camera camera = Camera.main;
            Vector3 camRotation = camera.transform.localEulerAngles;
            Debug.Log(camRotation);
            Quaternion camRotationFlatted = Quaternion.Euler(0, camRotation.y, camRotation.z);
            Debug.Log(camRotationFlatted.eulerAngles);
            forward = camRotationFlatted * Vector3.forward;
            right = Vector3.Cross(Vector3.up, forward);
        }

        [ContextMenu(nameof(CacheAnimator))] private void CacheAnimator() => animator = GetComponentInChildren<Animator>();

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + forward);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + right);

            Gizmos.color = Color.white;
        }

        private void Start()
        {
            camera = Camera.main;
            shootTimer = Timer.Create(shootCooldown, false).OnComplete(Shoot);
            shootTimer.Restart();

            shootFXPrefab.gameObject.SetActive(false);
            bulletPrefab.gameObject.SetActive(false);

            bloodFXPool = new ComponentPool<ParticleSystem>(25, bloodFXPrefab, "Blood FX", HideFlags.None, false);
            shootFXPool = new ComponentPool<ParticleSystem>(25, shootFXPrefab, "Shoot FX", HideFlags.None, false);
            bulletQueue = new ComponentPool<Bullet>(25, bulletPrefab, "Bullet", HideFlags.None, false);
        }

        private void Shoot()
        {
            Debug.Log("SHOOT");
            Bullet bullet = bulletQueue.Dequeue();
            bullet.transform.SetPositionAndRotation(bulletPrefab.transform.position, bulletPrefab.transform.rotation);
            bullet.Shoot(Aim(bullet), bulletSpeed);
            shootTimer.Restart();
            animator.SetTrigger("Shoot");
            ParticleSystem shootFX = shootFXPool.Dequeue(durationFX);
            shootFX.transform.SetPositionAndRotation(shootFXTransform.position, shootFXTransform.rotation);
        }

        private Vector3 Aim(Bullet bullet)
        {
            List<Unit> units = UnitManager.Instance.Units;
            Vector3 position = bullet.transform.position;
            Vector3 direction;
            Vector3 forward = transform.forward;
            for (int i = 0; i < units.Count; i++)
            {
                Vector3 targetPosition = units[i].transform.position;
                targetPosition.y = position.y;
                direction = position.Direction(targetPosition).normalized;
                float dot = Vector3.Dot(forward, direction);
                if (dot >= .9f) return direction;
            }
            direction = transform.forward;
            return direction;
        }

        public void EnqueueBullet(Bullet bullet, bool collide = false)
        {
            bullet.gameObject.SetActive(false);
            bulletQueue.Enqueue(bullet);
            if (!collide) return;

            ParticleSystem bloodFX = bloodFXPool.Dequeue(bloodFXPrefab.main.duration * 1.1f);
            bloodFX.transform.SetPositionAndRotation(bullet.transform.position, bullet.transform.rotation);
        }

        private void OnMove(InputValue input)
        {
            inputDirection = input.Get<Vector2>();

            animator.SetBool("Walking", inputDirection.sqrMagnitude > .1f);
            if (inputDirection.sqrMagnitude <= .1f) return;
            Vector3 direction = (forward * inputDirection.y + right * inputDirection.x).normalized;
            transform.forward = direction;

        }

        private void Update()
        {
            Vector3 direction = (forward * inputDirection.y + right * inputDirection.x).normalized;
            transform.position += Time.deltaTime * walkSpeed * direction;
        }

        public void Hit(int value)
        {
            health -= value;
            if (health <= 0) Die();
        }

        public void Heal(int value)
        {
            health += value;
        }
        public void Die()
        {
            Debug.Log("DIE");
        }

        public void Attack()
        {
            throw new System.NotImplementedException();
        }
    }
}
