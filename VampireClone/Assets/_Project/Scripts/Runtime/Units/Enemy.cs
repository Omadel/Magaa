using DG.Tweening;
using Etienne;
using UnityEngine;

namespace Magaa
{
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private float damage = 1;
        [SerializeField] private float maxHealth = 100;
        [SerializeField, ReadOnly] private float currentHealth;
        [SerializeField] private float walkSpeed = .4f;
        [SerializeField] private float updateRate = 1f;
        [SerializeField] private Rubis rubisPrefab;
        [SerializeField] ParticleSystem hitPrefab;
        private float updateTimer;
        private Transform playerTransform;
        private Animator animator;
        Material material;

        private void Awake()
        {
            SkinnedMeshRenderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
            int index = Random.Range(0, renderers.Length - 1);
            renderers[index].gameObject.SetActive(true);
            material = renderers[index].material;
            for (int i = 0; i < index; i++) renderers[i].gameObject.SetActive(false);
            for (int i = index + 1; i < renderers.Length; i++) renderers[i].gameObject.SetActive(false);
        }

        private void Start()
        {
            currentHealth = maxHealth;
            GameManager.Instance.AddEnemy(this);
            playerTransform = GameManager.Instance.Player.transform;
            transform.forward = GameManager.Instance.RoundWorldDirection(transform.Direction(playerTransform).normalized);
            animator = GetComponentInChildren<Animator>();
            animator.SetFloat("Walking Posture", Random.value);
            animator.CrossFadeInFixedTime("Walk", 0, 0, Random.value * animator.runtimeAnimatorController.animationClips[1].length);

        }

        private void Update()
        {
            if (Vector3.Distance(transform.position, playerTransform.position) < .2f)
            {
                GameManager.Instance.Player.Hit(damage);
                return;
            }
            transform.position += Time.deltaTime * walkSpeed * transform.forward;

            updateTimer += Time.deltaTime;
            if (updateTimer < 1 / updateRate) return;
            updateTimer -= 1 / updateRate;
            transform.forward = GameManager.Instance.RoundWorldDirection(transform.Direction(playerTransform).normalized);
        }
        Tween hitTween;
        internal void Hit(float damage)
        {
            currentHealth -= damage;
            hitTween?.Complete();
            hitTween = DOTween.To(() => material.GetFloat("_Hit"), x => material.SetFloat("_Hit", x), 1f, .1f).SetLoops(2,LoopType.Yoyo);
            if (currentHealth <= 0)
            {
                Die();
            }
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
            var fx = GameObject.Instantiate(hitPrefab, transform.position + Vector3.up, GameManager.Instance.Player.transform.rotation);
            Destroy(fx.gameObject, fx.main.duration);
        }
    }
}
