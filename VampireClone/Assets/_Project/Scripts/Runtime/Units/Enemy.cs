using DG.Tweening;
using Etienne;
using UnityEngine;
using UnityEngine.AI;

namespace Magaa
{
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private float damage = 1;
        [SerializeField] private float maxHealth = 100;
        [SerializeField, ReadOnly] private float currentHealth;
        [SerializeField] private float walkSpeed = .4f;
        [SerializeField] private float runSpeed = 3f;
        [SerializeField] private float updateRate = 1f;
        [SerializeField] private Rubis rubisPrefab;
        [SerializeField] private ParticleSystem hitPrefab;
        [SerializeField] private float attackRange = 1.5f;
        [Header("Audio")]
        [SerializeField] private Cue hitCue;

        private float updateTimer;
        private Transform playerTransform;
        private Animator animator;
        private Material material;
        private NavMeshAgent agent;

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
            currentHealth = maxHealth;
            GameManager.Instance.AddEnemy(this);
            playerTransform = GameManager.Instance.Player.transform;
            transform.forward = GameManager.Instance.RoundWorldDirection(transform.Direction(playerTransform).normalized);
            animator = GetComponentInChildren<Animator>();
            animator.SetFloat("Posture", Random.value);
            animator.CrossFadeInFixedTime("Walk", 0, 0, Random.value * animator.runtimeAnimatorController.animationClips[1].length);

        }

        internal void SetStats(float health, float damage, bool isRunning)
        {
            maxHealth = health;
            currentHealth = maxHealth;
            this.damage = damage;
            agent.speed = isRunning ? runSpeed : walkSpeed;
            animator = GetComponentInChildren<Animator>();
            animator.SetBool("IsRunning", isRunning);
        }

        private void Update()
        {
            if (Vector3.Distance(transform.position, playerTransform.position) < attackRange)
            {
                animator.SetBool("Walking", false);
                agent.isStopped = true;
                return;
            }
            agent.isStopped = false;

            animator.SetBool("Walking", true); ;
            updateTimer += Time.deltaTime;
            if (updateTimer < 1 / updateRate) return;
            updateTimer -= 1 / updateRate;
            agent.SetDestination(playerTransform.position);
            animator.transform.forward = GameManager.Instance.RoundWorldDirection(transform.Direction(playerTransform).normalized);
        }

        public void Attack()
        {
            GameManager.Instance.Player.Hit(damage);
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
            else
            {
            }
            hitCue.Play(transform.position);
        }

        private void Die()
        {
            hitTween?.Kill();
            GameManager.Instance.RemoveEnemy(this);
            GameObject.Instantiate(rubisPrefab, transform.position, Quaternion.identity);
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
