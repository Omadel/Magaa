using Etienne;
using System.Collections.Generic;
using UnityEngine;

namespace Magaa
{
    public class MagazineDisplay : MonoBehaviour
    {
        [SerializeField] private Material ammoMaterial, usedAmmoMaterial;
        [SerializeField] private int maxAmmo = 6;
        [SerializeField, ReadOnly] private int currentAmmo;
        [SerializeField, ReadOnly] private MeshRenderer ammoTemplate;


        private List<MeshRenderer> ammoRenderers = new List<MeshRenderer>();
        private List<MeshRenderer> activeAmmoRenderers = new List<MeshRenderer>();

        private void OnValidate()
        {
            if (Application.isPlaying) return;
            ammoTemplate = transform.GetChild(0).GetComponent<MeshRenderer>();
        }

        private void Start()
        {
            ammoTemplate.gameObject.SetActive(false);
        }

        public void SetMaxAmmo(int amount, Mesh ammoMesh)
        {
            maxAmmo = amount;
            for (int i = ammoRenderers.Count; i < maxAmmo; i++)
            {
                Debug.Log(i);
                MeshRenderer ammo = Instantiate(ammoTemplate, transform);
                ammo.gameObject.SetActive(true);
                ammo.GetComponent<MeshFilter>().mesh = ammoMesh;
                ammoRenderers.Add(ammo);
            }
            for (int i = activeAmmoRenderers.Count; i < Mathf.Min(maxAmmo, ammoRenderers.Count); i++)
            {
                MeshRenderer ammo = ammoRenderers[i];
                ammo.gameObject.SetActive(true);
                ammo.GetComponent<MeshFilter>().mesh = ammoMesh;
                activeAmmoRenderers.Add(ammo);
            }
            for (int i = maxAmmo; i < ammoRenderers.Count; i++)
            {
                ammoRenderers[i].gameObject.SetActive(false);
            }
            Reload();
        }

        public void Reload()
        {
            currentAmmo = maxAmmo;
            for (int i = 0; i < maxAmmo; i++)
            {
                activeAmmoRenderers[i].material = ammoMaterial;
            }
        }
        public void Shoot()
        {
            currentAmmo--;
            if (currentAmmo < 0) return;
            activeAmmoRenderers[currentAmmo].material = usedAmmoMaterial;
        }
    }
}
