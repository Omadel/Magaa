using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Etienne;
using UnityEngine;
using UnityEngine.AI;

namespace Magaa
{
    public class BruteBoss : MonoBehaviour
    {

        [SerializeField] private float maxHealth = 100;
        [SerializeField] private ParticleSystem hitPrefab;
        [SerializeField, ReadOnly] private float currentHealth;
        [SerializeField] ParticleSystem flameThrower;
        [SerializeField] private Cue hitCue;
        [SerializeField] private Cue deathCue;

        private Animator animator;
        private Material material;
        private NavMeshAgent agent;
        Transform playerTransform;

        private void Awake()
        {
            SkinnedMeshRenderer renderer = GetComponentInChildren<SkinnedMeshRenderer>(true);
            material = renderer.material;
            //agent = GetComponent<NavMeshAgent>();
            //agent.updateRotation = false;
            //agent.updateUpAxis = false;
            //agent.speed = walkSpeed;
            //agent.acceleration = 1000;
            //agent.autoBraking = false;
        }

        private void Start()
        {
            currentHealth = maxHealth;
            playerTransform = GameManager.Instance.Player.transform;
            transform.forward = GameManager.Instance.RoundWorldDirection(transform.Direction(playerTransform).normalized);
            animator = GetComponentInChildren<Animator>();
            flameThrower.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }


        private void Update()
        {

            //agent.SetDestination(playerTransform.position);
            animator.transform.forward = GameManager.Instance.RoundWorldDirection(transform.Direction(playerTransform).normalized);
            transform.position += animator.transform.forward * Time.deltaTime;
        }

        private void OnParticleCollision(GameObject other)
        {
            if (!other.TryGetComponent(out Bullet bullet)) return;
            Hit(bullet.Damage);
            ParticleSystem fx = GameObject.Instantiate(hitPrefab, transform.position + Vector3.up, GameManager.Instance.Player.transform.rotation);
            Destroy(fx.gameObject, fx.main.duration);
        }
        private Tween hitTween;
        internal void Hit(float damage)
        {
            currentHealth -= damage;
            hitTween?.Complete();
            hitTween = DOTween.To(() => material.GetFloat("_Hit"), x => material.SetFloat("_Hit", x), 1f, .1f).SetLoops(2, LoopType.Yoyo);
            if (currentHealth <= 0)
            {
                Die();
            }
            hitCue.Play(transform.position);
        }

        private void Die()
        {
            deathCue.Play(transform.position);
            GetComponent<Collider>().enabled = false;
            //GameManager.Instance.RemoveEnemy(this);
            //GameObject.Instantiate(rubisPrefab, transform.position, Quaternion.identity);
            animator.Play("Die");
            transform.DOMoveY(-1.2f, 2f).SetDelay(5f).OnComplete(Destroy);
        }
        void Destroy()
        {
            hitTween?.Kill();
            transform.DOKill();
            GameObject.Destroy(gameObject);
        }

        void StartFire()
        {
            flameThrower.Play(true);
        }
        void StopFire()
        {
            flameThrower.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}
