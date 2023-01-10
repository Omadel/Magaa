using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace VampireClone
{
    public class Joystick : MonoBehaviour
    {
        [SerializeField] private float fadeDuratiuon = .1f;

        private Image touchPosition;
        private bool shouldUpdateJoystick;
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private List<RaycastResult> raycastResults = new List<RaycastResult>();
        private Tween fadeTween;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            Player.Instance.SetJoystick(this);
            touchPosition = transform.GetChild(0).GetComponent<Image>();
            Disable();
        }

        private void ProcessTouchPosition(Vector2 position)
        {
            // Return if not enabled
            if (!enabled) return;

            // Calculate direction from touch position to the transform's position
            Vector3 touchDirection = (position - (Vector2)transform.position).normalized;
            // Set the direction on the Player instance
            Player.Instance.SetDirection(touchDirection.normalized);

            // Clamp the touch position to the min/max bounds of the rectTransform
            Vector2 clampedPosition = ClampToCircle(position, transform.position, rectTransform.rect.size.x * .5f);
            // Set the position of the touchDelta object
            touchPosition.transform.position = clampedPosition;
        }

        private Vector3 ClampToCircle(Vector3 position, Vector3 center, float radius)
        {
            Vector3 direction = position - center;
            float magnitude = direction.magnitude;
            bool isInsideTheCircle = magnitude <= radius;
            float angle = Vector3.SignedAngle(Vector3.up, direction, Vector3.forward);
            // Round the angle to the nearest multiple of angleSteps
            int roundedAngle = Mathf.RoundToInt(angle / Player.Instance.AngleSteps) * Player.Instance.AngleSteps;
            // Use Quaternion to rotate the object by the rounded angle
            direction = Quaternion.Euler(0, 0, roundedAngle) * Vector3.up;
            return center + (direction.normalized * (isInsideTheCircle ? magnitude : radius));
        }

        private bool TryEnablejoystick(Vector2 position)
        {
            if (EventSystem.current == null)
            {
                Debug.LogError("EventSystem.current is null");
                return false;
            }
            PointerEventData eventData = new PointerEventData(EventSystem.current) { position = position };
            raycastResults.Clear();
            EventSystem.current.RaycastAll(eventData, raycastResults);
            bool blockingUIObjectHit = raycastResults.Count > 0;
            if (blockingUIObjectHit) return false;

            shouldUpdateJoystick = false;
            transform.position = position;
            touchPosition.transform.localPosition = Vector3.zero;
            Enable();
            return true;
        }

        public void Press(InputValue input)
        {
            if (EventSystem.current.alreadySelecting) return;
            bool isPressed = input.Get<float>() > 0f;
            if (isPressed) shouldUpdateJoystick = true;
            else Disable();
        }

        private void Disable()
        {
            fadeTween?.Complete();
            fadeTween = canvasGroup.DOFade(0, fadeDuratiuon);
            Player.Instance.SetDirection(Vector2.zero);
            enabled = false;
        }

        private void Enable()
        {
            fadeTween?.Complete();
            fadeTween = canvasGroup.DOFade(1f, fadeDuratiuon);
            enabled = true;
        }

        internal void ChangePosition(Vector2 position)
        {
            if (EventSystem.current.currentSelectedGameObject != null) return;

            if (shouldUpdateJoystick) TryEnablejoystick(position);
            else ProcessTouchPosition(position);
        }
    }
}
