using UnityEngine;

namespace Magaa
{
    public class Bullet : MonoBehaviour
    {
        private float speed, damage;

        public void Shoot(Vector3 forward, float bulletSpeed, float bulletDamage)
        {
            speed = bulletSpeed;
            damage = bulletDamage;
            transform.forward = forward;
        }

        private void Update()
        {
            transform.position += speed * Time.deltaTime * transform.forward;
        }

        private void FixedUpdate()
        {
            Ray ray = new Ray(transform.position - transform.forward * transform.localScale.z, transform.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, transform.localScale.z * 2)) return;
            if (!hit.transform.TryGetComponent<Enemy>(out Enemy enemy)) return;
            enemy.Hit(damage);
            GameObject.Destroy(this.gameObject);
        }
    }
}
