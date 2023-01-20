using Etienne;
using System.Collections;
using UnityEngine;

namespace Magaa
{
    public class Weapon : MonoBehaviour
    {
        public float AttackDuration => attackDuration;
        public WeaponData Data => data;

        [SerializeField, ReadOnly] private float attackDuration;
        [SerializeField] private WeaponData data;
        [SerializeField] private ParticleSystem[] particleSystems;
        [SerializeField] private Transform shootTransform;
        [SerializeField] private ParticleSystem bulletSystem;
        private Bullet bullet;


        public void StopShooting()
        {
            particleSystems[0].Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        private void Start()
        {
            bullet = bulletSystem.gameObject.AddComponent<Bullet>();
            bullet.SetDamage(data.BulletDamage);
            StopShooting();
        }

        public void StartShooting(float additionnalSpeed)
        {
            StartCoroutine(StartShootingRoutine(additionnalSpeed));
        }

        private IEnumerator StartShootingRoutine(float additionnalSpeed)
        {
            attackDuration = 1 / (data.FireRate * additionnalSpeed);
            yield return new WaitForSeconds(attackDuration);
            foreach (ParticleSystem particleSystem in particleSystems)
            {
                particleSystem.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                ParticleSystem.MainModule main = particleSystem.main;
                main.duration = AttackDuration;
                if (particleSystem == bulletSystem)
                {
                    ParticleSystem.MinMaxCurve startSpeed = main.startSpeed;
                    startSpeed.constant = data.BulletSpeed;
                }
                particleSystem.Play(false);
            }
        }

        private void Update()
        {
            shootTransform.forward = GameManager.Instance.Player.transform.forward;
        }

    }
}
