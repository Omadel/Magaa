using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Magaa
{
    public class Rubis : MonoBehaviour
    {
        [SerializeField] int value = 1;
        private void OnTriggerEnter(Collider other)
        {
            Harvest();
        }

        private void Harvest()
        {
            GameManager.Instance.HarvestRubis(value);
            GameObject.Destroy(gameObject);
        }
    }
}
