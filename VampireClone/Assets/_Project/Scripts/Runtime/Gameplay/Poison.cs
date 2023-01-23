using System.Collections;
using UnityEngine;

namespace Magaa
{
    public class Poison : MonoBehaviour
    {
        public float ImparingForce => imparedForce;
        [SerializeField, Range(0f, 1f)] private float imparedForce = .5f;
        [SerializeField] private float puddleDuration;
        [SerializeField] private ParticleSystem FX;
        [SerializeField] private new Collider collider;

        private Transform parent;
        private Vector3 positionOffset;
        private Quaternion rotationOffset;
        private void Awake()
        {
            parent = transform.parent;
            positionOffset = transform.localPosition;
            rotationOffset = transform.localRotation;
            collider.enabled = false;
            FX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private IEnumerator DurationRoutine()
        {
            yield return new WaitForSeconds(puddleDuration);
            Disable();
        }

        public void Disable()
        {
            FX.Stop(true);
            collider.enabled = false;
            transform.SetParent(parent);
        }

        public void Enable()
        {
            FX.Play(true);
            collider.enabled = true;
            transform.localPosition = positionOffset;
            transform.localRotation = rotationOffset;
            transform.SetParent(null);
            StartCoroutine(DurationRoutine());
        }
    }
}
