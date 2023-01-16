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
        [SerializeField] private Rubis rubisPrefab;

        private void Start()
        {
            currentHealth = maxHealth;
            GameManager.Instance.AddEnemy(this);
        }

        private void Update()
        {
            Transform playerTransform = GameManager.Instance.Player.transform;
            if(Vector3.Distance(transform.position, playerTransform.position) < .2f)
            {
                GameManager.Instance.Player.Hit(damage);
                return;
            }
            transform.forward = GameManager.Instance.RoundWorldDirection(transform.Direction(playerTransform).normalized);
            transform.position += Time.deltaTime * walkSpeed * transform.forward;
        }

        internal void Hit(float damage)
        {
            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            GameManager.Instance.RemoveEnemy(this);
            GameObject.Instantiate(rubisPrefab, transform.position, Quaternion.identity);
            GameObject.Destroy(gameObject);
        }
    }
}
