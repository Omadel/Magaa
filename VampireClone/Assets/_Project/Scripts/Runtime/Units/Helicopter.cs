using DG.Tweening;
using Etienne;
using UnityEngine;

namespace Magaa
{
    public class Helicopter : MonoBehaviour
    {
        [Header("Animation")]
        [SerializeField] private Vector3 startPosition;
        [SerializeField, ReadOnly] private Vector3 midPosition;
        [SerializeField] private Vector3 endPosition;
        [SerializeField] private float startToMidDuration = 4f;
        [SerializeField] private AnimationCurve startToMidEase;
        [SerializeField] private float deployDuration = 5f;
        [SerializeField] private float midToEndDuration = 3f;
        [SerializeField] private AnimationCurve MidToEndEase;
        [Header("Blades")]
        [SerializeField] private Transform mainBlades;
        [SerializeField] private float mainBladesSpeed;
        [SerializeField] private Vector3 mainBladeAxis = new Vector3(0, 1, 0);
        [SerializeField] private Transform backBlades;
        [SerializeField] private float backBladesSpeed;
        [SerializeField] private Vector3 backBladeAxis = new Vector3(1, 0, 0);
        [Header("Doors")]
        [SerializeField] private Transform[] doors;
        [Header("Rope")]
        [SerializeField] private Transform rope;
        [SerializeField] private float ropeMaxSize = 5f;
        [Header("Extract Point")]
        [SerializeField] private GameObject extractPoint;

        private float mainAngle, backAngle;

        private void Awake()
        {
            midPosition = transform.localPosition;
            extractPoint.SetActive(false);
        }

        private void Start()
        {
            transform.localPosition = startPosition;
            transform.DOLocalMove(midPosition, startToMidDuration).OnComplete(Deploy).SetEase(startToMidEase);
        }

        private void Deploy()
        {
            foreach (Transform door in doors)
            {
                door.DOLocalMoveZ(-1.85f, .4f);
            }
            rope.DOScaleY(ropeMaxSize, deployDuration);
            rope.DOLocalMoveY(-ropeMaxSize * .5f, deployDuration).OnComplete(() => extractPoint.SetActive(true));
        }

        public void Extract(Transform extracted)
        {
            extractPoint.SetActive(false);
            extracted.SetParent(transform);
            extracted.DOLocalMoveY(0, midToEndDuration * .3f).SetEase(MidToEndEase);
            rope.DOScaleY(0, midToEndDuration * .3f).SetEase(MidToEndEase);
            rope.DOLocalMoveY(0, midToEndDuration * .3f).SetEase(MidToEndEase);
            transform.DOLocalMove(endPosition, midToEndDuration).OnComplete(CompleteGame).SetEase(MidToEndEase);
        }

        private void CompleteGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }

        private void Update()
        {
            mainAngle += Time.deltaTime * mainBladesSpeed;
            mainBlades.localRotation = Quaternion.Euler(mainAngle * mainBladeAxis.x, mainAngle * mainBladeAxis.y, mainAngle * mainBladeAxis.z);
            backAngle += Time.deltaTime * backBladesSpeed;
            backBlades.localRotation = Quaternion.Euler(backAngle * backBladeAxis.x, backAngle * backBladeAxis.y, backAngle * backBladeAxis.z);
        }

    }
}
