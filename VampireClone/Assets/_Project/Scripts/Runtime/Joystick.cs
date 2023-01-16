using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Magaa
{
    public class Joystick : MonoBehaviour
    {
        [SerializeField] private float fadeDuratiuon = .1f;

        private Image touchPosition;
        private bool isShowing;
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Tween fadeTween;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
            GetComponentInParent<Canvas>().transform.SetParent(null);
        }

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            touchPosition = transform.GetChild(0).GetComponent<Image>();
            InputHandler.Instance.OnTouchPressed += WillBeShown;
            InputHandler.Instance.OnTouchReleased += Hide;
            Hide();
        }

        private void Update()
        {
            if (isShowing) Show();
        }

        private void Show()
        {
            isShowing = false;
            if (InputHandler.Instance.IsOverUI()) return;
            InputHandler.Instance.OnPositionChanged += ProcessTouchPosition;
            transform.position = InputHandler.Instance.Position;
            touchPosition.transform.localPosition = Vector3.zero;
            canvasGroup.alpha = 1f;
        }

        private void WillBeShown()
        {
            isShowing = true;
        }

        private void Hide()
        {
            InputHandler.Instance.OnPositionChanged -= ProcessTouchPosition;
            canvasGroup.alpha = 0f;
            InputHandler.Instance.SetDirection(Vector3.zero);
        }

        private void ProcessTouchPosition(Vector2 uiPosition)
        {
            // Calculate direction from touch position to the transform's position
            InputHandler.Instance.SetDirection(uiPosition - (Vector2)transform.position);

            //Clamp the touchPosition's position to the rectTransform's height
            touchPosition.transform.position = ClampToCircle(uiPosition, transform.position, rectTransform.rect.size.y);
        }

        private Vector3 ClampToCircle(Vector3 position, Vector3 center, float radius)
        {
            Vector3 direction = position - center;
            float magnitude = direction.magnitude;
            bool isInsideTheCircle = magnitude <= radius;
            direction = GameManager.Instance.RoundUIDirection(direction);
            return center + (direction.normalized * (isInsideTheCircle ? magnitude : radius));
        }
    }
}
