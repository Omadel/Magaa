using DG.Tweening;
using Etienne;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

namespace VampireClone
{
    [DefaultExecutionOrder(-1)]
    public class Player : Etienne.Singleton<Player>, IDamageable, IAttacker
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
        [SerializeField, Range(0, 1)] private float recoil = .4f;
        [SerializeField] private Rig shootIK;
        [SerializeField] private ParticleSystem shootFX;
        [SerializeField] private float durationFX = 3f;
        [SerializeField] private Bullet bullet;
        [SerializeField] private float bulletSpeed;
        [SerializeField] ParticleSystem bloodSplat;

        [Header("Cached fields")]
        [SerializeField, Etienne.ReadOnly] private Animator animator;
        [SerializeField, Etienne.ReadOnly] private Vector3 forward;
        [SerializeField, Etienne.ReadOnly] private Vector3 right;

        private Vector2 inputDirection;
        private new Camera camera;
        private Timer shootTimer;
        private Sequence shootSequence;
        private Queue<ParticleSystem> shootFXQueue = new Queue<ParticleSystem>();
        private Queue<Bullet> bulletQueue = new Queue<Bullet>();

        private void Reset()
        {
            animator = GetComponentInChildren<Animator>();
            CalculateDirectionVectors();
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
            shootFX.gameObject.SetActive(false);

            shootFXQueue.Enqueue(shootFX);
            for (int i = 0; i < 100; i++)
            {
                ParticleSystem fx = Instantiate(shootFX, shootFX.transform.parent);
                shootFXQueue.Enqueue(fx);
                fx.gameObject.hideFlags = HideFlags.HideInHierarchy;
            }

            bullet.gameObject.SetActive(false);
            bulletQueue.Enqueue(bullet);
            for (int i = 0; i < 100; i++)
            {
                Bullet bullet = Instantiate(this.bullet, this.bullet.transform.parent);
                bulletQueue.Enqueue(bullet);
                bullet.gameObject.hideFlags = HideFlags.HideInHierarchy;
            }
        }

        private void Shoot()
        {
            Bullet bullet = bulletQueue.Dequeue();
            bullet.Shoot(Aim(bullet), bulletSpeed);
            shootTimer.Restart();
            shootSequence?.Complete();
            shootSequence = DOTween.Sequence();
            shootSequence.Append(DOTween.To(() => shootIK.weight, v => shootIK.weight = Mathf.RoundToInt(v * 4) / 4f, recoil, .4f)).SetEase(Ease.OutExpo);
            shootSequence.Append(DOTween.To(() => shootIK.weight, v => shootIK.weight = Mathf.RoundToInt(v * 4) / 4f, 0f, .4f));

            StartCoroutine(ShootFX());
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
            //todo  Blood Pool
            GameObject.Instantiate(bloodSplat, bullet.transform.position, bullet.transform.rotation);
        }

        private IEnumerator ShootFX()
        {
            ParticleSystem fx = shootFXQueue.Dequeue();
            fx.gameObject.SetActive(true);
            yield return new WaitForSeconds(durationFX);
            fx.gameObject.SetActive(false);
            shootFXQueue.Enqueue(fx);
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
