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


        [Header("Player")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private int health = 100;
        [SerializeField] private float shootCooldown = 1f;
        [SerializeField] private int damage = 10;
        [Header("Shoot")]
        [SerializeField, Range(0, 1)] private float recoil = .4f;
        [SerializeField] private Rig shootIK;
        [SerializeField] private ParticleSystem shootFX;
        [SerializeField] float durationFX = 3f;
        [SerializeField] Bullet bullet;
        [SerializeField] float bulletSpeed;

        [Header("Cached fields")]
        [SerializeField, Etienne.ReadOnly] private Animator animator;
        [SerializeField, Etienne.ReadOnly] private Vector3 forward;
        [SerializeField, Etienne.ReadOnly] private Vector3 right;

        private Vector2 inputDirection;
        private new Camera camera;
        private Timer shootTimer;
        private Tween shootTween;
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
            for (int i = 0; i < 15; i++)
            {
                ParticleSystem fx = Instantiate(shootFX, shootFX.transform.parent);
                shootFXQueue.Enqueue(fx);
                fx.hideFlags = HideFlags.HideAndDontSave;
            }

            for (int i = 0; i < 300; i++)
            {
                var bullet = Instantiate(this.bullet, this.bullet.transform.parent);
                bulletQueue.Enqueue(bullet);
                //bullet.hideFlags = HideFlags.HideAndDontSave;
            }
        }

        private void Shoot()
        {
            Debug.Log("Shooit");
            shootTimer.Restart();
            shootTween?.Complete();
            shootTween = DOTween.To(() => shootIK.weight, v => shootIK.weight = Mathf.RoundToInt(v * 4) / 4f, recoil, .4f)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.InFlash);
            StartCoroutine(ShootFX());
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
            //todo add Buffer
            animator.SetBool("Walking", inputDirection.sqrMagnitude > .1f);
        }

        private void Update()
        {
            Vector3 direction = (forward * inputDirection.y + right * inputDirection.x).normalized;
            transform.position += Time.deltaTime * walkSpeed * direction;
            if (direction != Vector3.zero) transform.forward = direction;

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
