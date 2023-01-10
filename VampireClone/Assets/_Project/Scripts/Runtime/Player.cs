using Etienne;
using Etienne.Pools;
using UnityEngine;
using UnityEngine.InputSystem;

namespace VampireClone
{
    [DefaultExecutionOrder(-1)]
    public class Player : Singleton<Player>, IDamageable, IAttacker
    {
        public int Health => health;
        public int Damage => damage;
        public int AngleSteps => angleSteps;

        [Header("Player")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private int health = 100;
        [SerializeField] private float shootingSpeed = 1f;
        [SerializeField] private int damage = 10;
        [Header("Shoot")]
        [SerializeField] private ParticleSystem shootFXPrefab;
        [SerializeField] private Transform shootFXTransform;
        [SerializeField] private float durationFX = 3f;
        [SerializeField] private Bullet bulletPrefab;
        [SerializeField] private float bulletSpeed;
        [SerializeField] private float maxAimAngle = 40f;
        [SerializeField] private ParticleSystem bloodFXPrefab;
        [SerializeField] private AnimationClip shootingClip;
        [SerializeField] private int angleSteps = 45;

        [Header("Cached fields")]
        [SerializeField, ReadOnly] private Animator animator;
        [SerializeField, ReadOnly] private Vector3 forward;
        [SerializeField, ReadOnly] private Vector3 right;

        private Vector2 inputDirection;
        private Camera camera;
        private Timer shootTimer;
        private ComponentPool<ParticleSystem> bloodFXPool, shootFXPool;
        private ComponentPool<Bullet> bulletQueue;
        private Joystick joystick;

        #region Editor
        private void Reset()
        {
            CacheAnimator();
            CalculateDirectionVectors();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying) return;
            shootTimer?.SetDuration(1 / shootingSpeed);
            shootTimer?.Restart();
            animator.SetFloat("ShootingSpeed", Mathf.Max(1f, shootingSpeed / shootingClip.length));
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

        [ContextMenu(nameof(CacheAnimator))] private void CacheAnimator() => animator = GetComponentInChildren<Animator>();

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + forward);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + right);
            Gizmos.color = Color.white;
        }
        #endregion

        private void Start()
        {
            camera = Camera.main;
            shootTimer = Timer.Create(1 / shootingSpeed, false).OnComplete(Shoot);
            shootTimer.Restart();

            shootFXPrefab.gameObject.SetActive(false);
            bulletPrefab.gameObject.SetActive(false);

            bloodFXPool = new ComponentPool<ParticleSystem>(25, bloodFXPrefab, "Blood FX", HideFlags.None, false);
            shootFXPool = new ComponentPool<ParticleSystem>(25, shootFXPrefab, "Shoot FX", HideFlags.None, false);
            bulletQueue = new ComponentPool<Bullet>(25, bulletPrefab, "Bullet", HideFlags.None, false);

            animator.SetFloat("ShootingSpeed", Mathf.Max(1f, shootingSpeed / shootingClip.length));
        }

        private void Shoot()
        {
            Bullet bullet = bulletQueue.Dequeue();
            bullet.transform.SetPositionAndRotation(bulletPrefab.transform.position, bulletPrefab.transform.rotation);
            bullet.Shoot(GetDirectionToClosestUnitInAngle(bullet), bulletSpeed);

            shootTimer.Restart();
            animator.Play("Shoot", 1);

            ParticleSystem shootFX = shootFXPool.Dequeue(durationFX);
            shootFX.transform.SetPositionAndRotation(shootFXTransform.position, shootFXTransform.rotation);
        }

        private Vector3 GetDirectionToClosestUnitInAngle(Bullet bullet)
        {
            // Initialize the direction 
            Vector3 direction = transform.forward;
            Vector3 forward = transform.forward;
            float closestAngle = 180;
            //iterate over the units
            foreach (Unit unit in UnitManager.Instance.Units)
            {
                Vector3 targetPosition = unit.transform.position;
                //update the y position 
                targetPosition.y = bullet.transform.position.y;

                Vector3 directionToTarget = targetPosition - bullet.transform.position;
                directionToTarget.Normalize();

                // calculate the angle
                float angle = Vector3.Angle(forward, directionToTarget);
                // Check if angle is less than the max allowed angle
                if (angle < closestAngle && angle < maxAimAngle)
                {
                    closestAngle = angle;
                    direction = directionToTarget;
                }
            }
            return direction;
        }

        public void EnqueueBullet(Bullet bullet, bool collide = false)
        {
            bullet.gameObject.SetActive(false);
            bulletQueue.Enqueue(bullet);
            if (!collide) return;

            ParticleSystem bloodFX = bloodFXPool.Dequeue(bloodFXPrefab.main.duration * 1.1f);
            bloodFX.transform.SetPositionAndRotation(bullet.transform.position, bullet.transform.rotation);
        }

        private void OnMove(InputValue input) => SetDirection(input.Get<Vector2>());

        public void SetDirection(Vector2 direction)
        {
            inputDirection = direction;

            bool isMoving = direction.sqrMagnitude > .1f;
            animator.Play(isMoving ? "Walk" : "Idle", 0);
            if (!isMoving) return;

            Vector3 targetDirection = (forward * direction.y) + (right * direction.x);
            targetDirection.Normalize();
            float angle = Vector3.SignedAngle(forward, targetDirection, Vector3.up);
            // Round the angle to the nearest multiple of angleSteps
            int roundedAngle = Mathf.RoundToInt(angle / angleSteps) * angleSteps;
            // Use Quaternion to rotate the object by the rounded angle
            transform.forward = Quaternion.Euler(0, roundedAngle, 0) * forward;
        }

        public void SetJoystick(Joystick joystick) => this.joystick = joystick;
        private void OnTouchPress(InputValue input) => joystick.Press(input);
        private void OnTouchPosition(InputValue input) => joystick.ChangePosition(input.Get<Vector2>());

        private void Update()
        {
            transform.position += Time.deltaTime * walkSpeed * transform.forward * inputDirection.magnitude;
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
