using DG.Tweening;
using Etienne;
using Etienne.Pools;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Magaa
{
    public class BruteBoss : MonoBehaviour, IDamageable
    {

        [SerializeField] private float maxHealth = 100;
        [SerializeField] private ParticleSystem hitPrefab;
        [SerializeField, ReadOnly] private float currentHealth;
        [SerializeField] private float updateRate = 1f;
        [SerializeField] private LayerMask attackMask;
        [Header("FlameThrower")]
        [SerializeField] private ParticleSystem flameThrower;
        [SerializeField, MinMaxRange(0f, 35f)] private Range flameThrowerRange = new Range(0, 2);
        [SerializeField] private int flameThrowerDamage = 10;
        [SerializeField] Cue startFlameThrower, loopFlameThrower, stopFlamethrower;
        [Header("GroundSlam")]
        [SerializeField] private ParticleSystem groundSlam;
        [SerializeField, MinMaxRange(0f, 35f)] private Range groundSlamRange = new Range(9f, 11f);
        [SerializeField] private float groundSlamExplosionRange = 2f;
        [SerializeField] private int groundSlamDamage = 60;
        [SerializeField]        Cue slamCue;
        [Header("ThrowAttack")]
        [SerializeField] private ParticleSystem throwAttack;
        [SerializeField] private ParticleSystem throwAttackWarning;
        [SerializeField, MinMaxRange(0f, 35f)] private Range throwAttackRange = new Range(9f, 17f);
        [SerializeField] private float throwAttackExplosionRange = 2f;
        [SerializeField] private int throwAttackDamage = 50;
        [SerializeField] Cue throwCue, throwCrashCue;
        [Header("Audio")]
        [SerializeField] private Cue hitCue;
        [SerializeField] private Cue deathCue;

        private enum Attack { None, Throw, Slam, FlameThrower }

        private Attack lastAttack = Attack.None;
        private float updateTimer;
        private Animator animator;
        private Material material;
        private NavMeshAgent agent;
        private Transform playerTransform;
        private bool isAttacking = false;

        private void Awake()
        {
            SkinnedMeshRenderer renderer = GetComponentInChildren<SkinnedMeshRenderer>(true);
            material = renderer.material;
            agent = GetComponent<NavMeshAgent>();
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.acceleration = 1000;
            agent.autoBraking = false;
        }

        private void Start()
        {
            currentHealth = maxHealth;
            playerTransform = GameManager.Instance.Player.transform;
            transform.forward = GameManager.Instance.RoundWorldDirection(transform.Direction(playerTransform).normalized);
            animator = GetComponentInChildren<Animator>();
            flameThrower.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            flameThrower.gameObject.AddComponent<Bullet>().SetDamage(flameThrowerDamage);
            groundSlam.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            throwAttack.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            throwAttackWarning.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            agent.Warp(transform.position);
        }


        private void Update()
        {

            if (isAttacking) return;

            float distance = Vector3.Distance(transform.position, playerTransform.position);

            if (throwAttackRange.Contains(distance) && lastAttack != Attack.Throw)
            {
                transform.forward = GameManager.Instance.RoundWorldDirection(transform.Direction(playerTransform).normalized);
                lastAttack = Attack.Throw;
                isAttacking = true;
                agent.isStopped = true;
                animator.Play("Throw");
                return;
            }
            if (groundSlamRange.Contains(distance) && lastAttack != Attack.Slam)
            {
                transform.forward = GameManager.Instance.RoundWorldDirection(transform.Direction(playerTransform).normalized);
                lastAttack = Attack.Slam;
                isAttacking = true;
                agent.isStopped = true;
                animator.Play("Jump Attack");
                return;
            }
            if (flameThrowerRange.Contains(distance) && lastAttack != Attack.FlameThrower)
            {
                transform.forward = GameManager.Instance.RoundWorldDirection(transform.Direction(playerTransform).normalized);
                lastAttack = Attack.FlameThrower;
                isAttacking = true;
                agent.isStopped = true;
                animator.Play("Roar");
                return;
            }

            Vector3 targetPosition = playerTransform.position;

            if (distance < flameThrowerRange.Min || (distance < flameThrowerRange.Max && lastAttack == Attack.FlameThrower))
            {
                targetPosition = transform.position - transform.position.Direction(targetPosition);
            }

            updateTimer += Time.deltaTime;
            if (updateTimer < 1 / updateRate) return;
            updateTimer -= 1 / updateRate;
            agent.SetDestination(targetPosition);
            transform.forward = GameManager.Instance.RoundWorldDirection(transform.position.Direction(targetPosition).normalized);

        }

        private void OnParticleCollision(GameObject other)
        {
            if (!other.TryGetComponent(out Bullet bullet)) return;
            Hit(bullet.Damage);
            ParticleSystem fx = GameObject.Instantiate(hitPrefab, transform.position + Vector3.up, GameManager.Instance.Player.transform.rotation);
            Destroy(fx.gameObject, fx.main.duration);
        }
        private Tween hitTween;

        public int Health => throw new System.NotImplementedException();

        public void Hit(int damage)
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

        public void Die()
        {
            deathCue.Play(transform.position);
            GetComponent<Collider>().enabled = false;
            //GameObject.Instantiate(rubisPrefab, transform.position, Quaternion.identity);
            animator.Play("Die");
            transform.DOMoveY(-1.2f, 2f).SetDelay(5f).OnComplete(Destroy);
        }

        private void Destroy()
        {
            hitTween?.Kill();
            transform.DOKill();
            GameObject.Destroy(gameObject);
        }

        private void StartIdle()
        {
            agent.isStopped = false;
            isAttacking = false;
        }

        private void StartFire()
        {
            flameThrower.Play(true);
            startFlameThrower.Play(transform.position);
            StartCoroutine(FireRoutine(.2f));
        }
        AudioSource loopFlame;
        IEnumerator FireRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            loopFlame = AudioSourcePool.PlayLooped(loopFlameThrower);
        }

        private void StopFire()
        {
            flameThrower.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            loopFlame.Stop();
            stopFlamethrower.Play(transform.position);
        }

        private void GroundSlam()
        {
            groundSlam.Play(true);
            Ray ray = new Ray(groundSlam.transform.position, groundSlam.transform.forward);
            RaycastHit[] hits = Physics.SphereCastAll(ray, groundSlamExplosionRange, groundSlamExplosionRange, attackMask, QueryTriggerInteraction.Ignore);
            foreach (RaycastHit hit in hits)
            {
                if (!hit.collider.TryGetComponent(out IDamageable damagable)) continue;
                damagable.Hit(groundSlamDamage);
            }
            slamCue.Play();
        }

        private void ResetPositionAfterGroundSlam()
        {
            NavMesh.SamplePosition(animator.transform.GetChild(0).position, out NavMeshHit hit, 10, NavMesh.AllAreas);
            transform.position = hit.position;
        }

        private void Throw()
        {
            throwAttack.Play(true);
            throwCue.Play();
        }

        private void StartWarning()
        {
            throwAttackWarning.Play(true);
        }

        private void StopWarning()
        {
            throwAttackWarning.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            Ray ray = new Ray(throwAttackWarning.transform.position, throwAttackWarning.transform.forward);
            RaycastHit[] hits = Physics.SphereCastAll(ray, throwAttackExplosionRange, throwAttackExplosionRange, attackMask, QueryTriggerInteraction.Ignore);
            foreach (RaycastHit hit in hits)
            {
                if (!hit.collider.TryGetComponent(out IDamageable damagable)) continue;
                damagable.Hit(throwAttackDamage);
            }
            throwCrashCue.Play();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            using (new UnityEditor.Handles.DrawingScope(Color.red, transform.localToWorldMatrix))
            {
                UnityEditor.Handles.DrawWireArc(throwAttackWarning.transform.localPosition, Vector3.up, Vector3.forward, 360f, throwAttackExplosionRange);
                UnityEditor.Handles.DrawWireArc(Vector3.zero, Vector3.up, Vector3.forward, 90f, throwAttackRange.Min);
                UnityEditor.Handles.DrawWireArc(Vector3.zero, Vector3.up, Vector3.forward, -90f, throwAttackRange.Min);
                UnityEditor.Handles.DrawWireArc(Vector3.zero, Vector3.up, Vector3.forward, 90f, throwAttackRange.Max);
                UnityEditor.Handles.DrawWireArc(Vector3.zero, Vector3.up, Vector3.forward, -90f, throwAttackRange.Max);
            }
            using (new UnityEditor.Handles.DrawingScope(Color.green, transform.localToWorldMatrix))
            {
                UnityEditor.Handles.DrawWireArc(groundSlam.transform.localPosition, Vector3.up, Vector3.forward, 360f, groundSlamExplosionRange);
                UnityEditor.Handles.DrawWireArc(Vector3.zero, Vector3.up, Vector3.forward, 90f, groundSlamRange.Min);
                UnityEditor.Handles.DrawWireArc(Vector3.zero, Vector3.up, Vector3.forward, -90f, groundSlamRange.Min);
                UnityEditor.Handles.DrawWireArc(Vector3.zero, Vector3.up, Vector3.forward, 90f, groundSlamRange.Max);
                UnityEditor.Handles.DrawWireArc(Vector3.zero, Vector3.up, Vector3.forward, -90f, groundSlamRange.Max);
            }
            using (new UnityEditor.Handles.DrawingScope(Color.blue, transform.localToWorldMatrix))
            {
                UnityEditor.Handles.DrawWireArc(Vector3.zero, Vector3.up, Vector3.forward, 90f, flameThrowerRange.Min);
                UnityEditor.Handles.DrawWireArc(Vector3.zero, Vector3.up, Vector3.forward, -90f, flameThrowerRange.Min);
                UnityEditor.Handles.DrawWireArc(Vector3.zero, Vector3.up, Vector3.forward, 90f, flameThrowerRange.Max);
                UnityEditor.Handles.DrawWireArc(Vector3.zero, Vector3.up, Vector3.forward, -90f, flameThrowerRange.Max);
            }
        }
#endif
    }
}
