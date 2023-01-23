using DG.Tweening;
using UnityEngine;

namespace Magaa
{
    public class Rubis : MonoBehaviour
    {
        [SerializeField] private int value = 1;
        [Header("Idle")]
        [SerializeField, Min(.01f)] private float idleDuration = .8f;
        [Header("Tween")]
        [SerializeField, Min(.01f)] private float duration = .8f;
        [SerializeField] private AnimationCurve ease;
        private bool isHarvested = false;
        private float tweenValue = 0f;
        private Vector3 startPosition;

        private void Start()
        {
            transform.DOMoveY(transform.position.y + 1, idleDuration).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).SetDelay(duration * Random.value);
            transform.DOLocalRotate(new Vector3(0, 180, 0), idleDuration, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear).SetDelay(duration * Random.value);
        }

        private void Update()
        {
            if (!GameManager.Instance.enabled) return;
            if (Vector3.Distance(transform.position, GameManager.Instance.Player.transform.position) < 3f && !isHarvested)
            {
                isHarvested = true;
                startPosition = transform.position;
            }
            if (!isHarvested) return;
            transform.DOKill();
            tweenValue += Time.deltaTime;
            if (tweenValue / duration >= 1f)
            {
                Harvest();
                return;
            }
            transform.position = Vector3.Slerp(startPosition, GameManager.Instance.Player.transform.position, ease.Evaluate(tweenValue / duration));
        }

        private void Harvest()
        {
            GameManager.Instance.HarvestRubis(value);
            GameObject.Destroy(gameObject);
        }

        private void OnDestroy()
        {
            transform.DOKill();
        }
    }
}
