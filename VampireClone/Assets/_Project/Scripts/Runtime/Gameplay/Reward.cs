using UnityEngine;

namespace Magaa
{
    public class Reward : MonoBehaviour
    {
        [SerializeField] private WeaponData weapon;
        private Player player;
        private bool isOpened = false;

        private void Start()
        {
            player = GameManager.Instance.Player;
        }

        private void Update()
        {
            if (!isOpened && Vector3.Distance(transform.position, player.transform.position) < .7f)
            {
                isOpened = true;
                Open();
            }
        }

        private void Open()
        {
            player.SetWeapon(weapon);
            GameObject.Destroy(gameObject);
        }
    }
}
