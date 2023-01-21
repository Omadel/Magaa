using UnityEngine;

namespace Magaa
{
    public class ExtractPoint : MonoBehaviour
    {
        [SerializeField] private Helicopter helicopter;
        [SerializeField] private float range = 3f;
        private Transform playerTransform;

        private void Start()
        {
            playerTransform = GameManager.Instance.Player.transform;
        }

        private void LateUpdate()
        {
            if (Vector3.Distance(playerTransform.position, transform.position) < range*.5f)
            {
                playerTransform.position = transform.position;
                helicopter.Extract(playerTransform.GetChild(0));
                GameManager.Instance.Player.enabled = false;

            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            using(new UnityEditor.Handles.DrawingScope(transform.localToWorldMatrix))
            {
                UnityEditor.Handles.DrawWireArc(Vector3.zero,Vector3.up,Vector3.forward, 360f, range);
            }
        }
#endif
    }
}
