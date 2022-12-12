using Etienne;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace VampireClone
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Unit : MonoBehaviour, IDamageable, IAttacker
    {
        public int Health => health;

        public int Damage => throw new System.NotImplementedException();

        [Header("Unit")]
        [SerializeField] private float walkSpeed = .4f;
        [SerializeField] private int health = 100;
        [SerializeField] private int damage = 5;
        [SerializeField] private float attackRange = 1f;

        [Header("Cached fields")]
        [SerializeField, ReadOnly] private Animator animator;
        [SerializeField, ReadOnly] private NavMeshAgent agent;
        [SerializeField, ReadOnly] private SkinnedMeshRenderer[] renderers;
        private WaitForSeconds waitForSeconds = new WaitForSeconds(.4f);

        private void Reset()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponentInChildren<Animator>();
            renderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        }

        private void Awake()
        {
            int index = Random.Range(0, renderers.Length - 1);
            renderers[index].gameObject.SetActive(true);
            for (int i = 0; i < index; i++) renderers[i].gameObject.SetActive(false);
            for (int i = index + 1; i < renderers.Length; i++) renderers[i].gameObject.SetActive(false);

            agent.updateRotation = false;
            UnitManager.Instance.AddUnit(this);
        }

        private IEnumerator Start()
        {
            agent.speed = walkSpeed;
            animator.SetFloat("Walking Posture", Random.value);

            while (enabled)
            {
                Vector3 destination = Player.Instance.transform.position;
                bool isWalking = Vector3.Distance(destination, transform.position) > 1f;
                animator.SetBool("Walking", isWalking);
                if (!isWalking)
                {
                    if (agent.enabled)
                    {
                        agent.isStopped = true;
                        agent.enabled = false;
                    }
                }
                else
                {
                    if (!agent.enabled)
                    {
                        agent.enabled = true;
                        agent.isStopped = false;
                    }
                    agent.SetDestination(destination);
                    Vector3 forward = transform.position.Direction(agent.destination).normalized;
                    if (forward != Vector3.zero) transform.forward = forward;
                }
                yield return waitForSeconds;
            }
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
            if (Vector3.Distance(Player.Instance.transform.position, transform.position) <= attackRange)
            {
                Debug.Log("Attack");
                Player.Instance.Hit(damage);
            }
        }

        private void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            UnityEditor.Handles.DrawWireArc(transform.position, transform.up, transform.forward, 360f, attackRange);
#endif
        }
    }
}
