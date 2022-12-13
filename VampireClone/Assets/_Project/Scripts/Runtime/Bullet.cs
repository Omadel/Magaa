using UnityEngine;

namespace VampireClone
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private int flatRatio = 20;
        [SerializeField] private TrailRenderer trailRenderer;
        [SerializeField] private new Rigidbody rigidbody;
        private float speed;
        private Vector3 position;
        private Transform parent;
        private Vector3 zero = Vector3.zero;


        public void Shoot(Vector3 forward, float bulletSpeed)
        {
            Reset();
            rigidbody.velocity = zero;
            rigidbody.angularVelocity = zero;
            transform.SetParent(null);
            gameObject.SetActive(true);
            speed = bulletSpeed;
            transform.forward = forward;
        }

        private void Reset()
        {
            trailRenderer.Clear();
            if (transform.parent != null)
            {
                parent = transform.parent;
                position = transform.localPosition;
            }
            else
            {
                transform.SetParent(parent);
                transform.localPosition = position;
            }
        }

        private void FixedUpdate()
        {
            rigidbody.MovePosition(transform.position + speed * Time.deltaTime * transform.forward);
            if (!GetComponent<Renderer>().isVisible) Enqueue();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!collision.gameObject.TryGetComponent(out Unit unit)) return;
            unit.Hit(Player.Instance.Damage);
            Enqueue(true);

        }

        private void Enqueue(bool collide = false)
        {
            Player.Instance.EnqueueBullet(this, collide);
        }
    }
}
