using DG.Tweening;
using Etienne;
using UnityEngine;
using UnityEngine.AI;

namespace Magaa
{
    public class Enemy : MonoBehaviour, IDamageable
    {
        public int Health => currentHealth;

        [SerializeField] private int damage = 1;
        [SerializeField] private int maxHealth = 100;
        [SerializeField, ReadOnly] private int currentHealth;
        [SerializeField] private float walkSpeed = .4f;
        [SerializeField] private float runSpeed = 3f;
        [SerializeField] private float updateRate = 1f;
        [SerializeField] private Pickup[] onDeathPickups;
        [SerializeField] private ParticleSystem hitPrefab;
        [SerializeField] private float attackRange = 1.5f;
        [Header("Puke")]
        [SerializeField] private ParticleSystem pukeThrower;
        [SerializeField] private Poison poisonPuddle;
        [SerializeField, ReadOnly] private bool isPuking = false;
        [SerializeField] private float pukingChance = .3f;
        [SerializeField] private float pukingDelay = 5f;
        [Header("Audio")]
        [SerializeField, Range(0f, 1f)] private float spawnSoundChance;
        [SerializeField] private Cue spawnCue;
        [SerializeField] private Cue warningRunningCue;
        [SerializeField] private Cue hitCue;
        [SerializeField] private Cue deathCue;

        private float pukingTimer;
        private float updateTimer;
        private Transform playerTransform;
        private Animator animator;
        private Material material;
        private NavMeshAgent agent;
        private AudioSource warningSource;

        [System.Serializable]
        private struct Pickup
        {
            [Range(0f, 1f)] public float Chance;
            public GameObject Prefab;
        }

        private void Awake()
        {
            SkinnedMeshRenderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
            int index = Random.Range(0, renderers.Length - 1);
            renderers[index].gameObject.SetActive(true);
            material = renderers[index].material;
            for (int i = 0; i < index; i++) renderers[i].gameObject.SetActive(false);
            for (int i = index + 1; i < renderers.Length; i++) renderers[i].gameObject.SetActive(false);
            agent = GetComponent<NavMeshAgent>();
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.speed = walkSpeed;
            agent.acceleration = 1000;
            agent.autoBraking = false;
        }

        private void Start()
        {
            enabled = false;
            currentHealth = maxHealth;
            GameManager.Instance.AddEnemy(this);
            playerTransform = GameManager.Instance.Player.transform;
            transform.forward = GameManager.Instance.RoundWorldDirection(transform.Direction(playerTransform).normalized);
            animator = GetComponentInChildren<Animator>();
            animator.SetFloat("Posture", Random.value);
            if (Random.value < spawnSoundChance) spawnCue.Play(transform.position);
            pukeThrower.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            pukingTimer = Random.value * pukingDelay;
            poisonPuddle.Disable();
        }

        internal void SetStats(int health, int damage, bool isRunning)
        {
            maxHealth = health;
            currentHealth = maxHealth;
            this.damage = damage;
            agent.speed = isRunning ? runSpeed : walkSpeed;
            animator = GetComponentInChildren<Animator>();
            animator.SetBool("IsRunning", isRunning);
            if (isRunning) warningSource = warningRunningCue.Play(transform.position);
        }

        private void Update()
        {
            if (isPuking)
            {
                return;
            }
            else
            {
                pukingTimer += Time.deltaTime;
                if (pukingTimer > pukingDelay && Random.value < pukingChance)
                {
                    pukingTimer -= pukingDelay;
                    isPuking = true;
                    animator.Play("Puke");
                    agent.isStopped = true;
                    poisonPuddle.Enable();
                    return;
                }
            }

            if (!agent.isStopped)
            {
                if (Vector3.Distance(transform.position, playerTransform.position) < attackRange * .5f)
                {
                    animator.SetBool("Walking", false);
                    agent.isStopped = true;
                    return;
                }
            }
            else
            {
                if (Vector3.Distance(transform.position, playerTransform.position) < attackRange)
                {
                    return;
                }

            }
            agent.isStopped = false;

            animator.SetBool("Walking", true);
            updateTimer += Time.deltaTime;
            if (updateTimer < 1 / updateRate) return;
            updateTimer -= 1 / updateRate;
            agent.SetDestination(playerTransform.position);
            animator.transform.forward = GameManager.Instance.RoundWorldDirection(transform.Direction(playerTransform).normalized);
        }

        private void StartBehaviour()
        {
            enabled = true;
        }

        private void StartPuke()
        {
            pukeThrower.Play(true);
        }

        private void StopPuke()
        {
            pukeThrower.Stop(true);
            isPuking = false;
        }

        public void Attack()
        {
            GameManager.Instance.Player.Hit(damage);
        }

        private Tween hitTween;
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
            hitTween?.Kill();
            deathCue.Play(transform.position);
            if (warningSource != null) warningSource.Stop();
            GameManager.Instance.RemoveEnemy(this);
            float random = Random.value;
            GameObject prefab = null;
            float totalChance = 0f;
            for (int i = 0; i < onDeathPickups.Length; i++)
            {
                float chance = onDeathPickups[i].Chance + totalChance;
                if (random <= chance)
                {
                    prefab = onDeathPickups[i].Prefab;
                    break;
                }
                totalChance += onDeathPickups[i].Chance;
            }
            GameObject.Instantiate(prefab, transform.position, Quaternion.identity);
            GameObject.Destroy(gameObject);
        }

        private void OnParticleCollision(GameObject other)
        {
            if (!other.TryGetComponent(out Bullet bullet)) return;
            Hit(bullet.Damage);
            ParticleSystem fx = GameObject.Instantiate(hitPrefab, transform.position + Vector3.up, GameManager.Instance.Player.transform.rotation);
            Destroy(fx.gameObject, fx.main.duration);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            using (new UnityEditor.Handles.DrawingScope(Color.red, transform.localToWorldMatrix))
            {
                UnityEditor.Handles.DrawWireArc(Vector3.zero, Vector3.up, Vector3.forward, 360f, attackRange);
            }
        }
#endif
    }
}
