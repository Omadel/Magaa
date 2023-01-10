using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace VampireClone
{
    public class Joystick : MonoBehaviour
    {
        private Image touchDelta;
        private bool shouldUpdateJoystick;
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private List<RaycastResult> raycastResults = new List<RaycastResult>();

        private void Awake()
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            Player.Instance.SetJoystick(this);
            touchDelta = transform.GetChild(0).GetComponent<Image>();
            Disable();
        }

        private void UpdateTouchDelta(Vector2 position)
        {
            // Calculate the direction from the transform's position to the touch position
            Vector3 direction = (position - (Vector2)transform.position).normalized;
            // Scale the direction by half the size of the rectTransform
            direction *= (rectTransform.rect.size * .5f);
            // Calculate the clamped position
            //Vector2 clampedPosition = ClampPositionWithinBounds(transform.position + direction);
            Vector2 clampedPosition = transform.position + direction;
            // Update the touchDelta's position
            touchDelta.transform.position = clampedPosition;
            // Set the direction for the player
            Player.Instance.SetDirection(direction.normalized);
        }

        private Vector2 ClampPositionWithinBounds(Vector2 position)
        {
            Vector2 min = (Vector2)transform.position - rectTransform.rect.size * .5f;
            Vector2 max = (Vector2)transform.position + rectTransform.rect.size * .5f;
            return new Vector2(Mathf.Clamp(position.x, min.x, max.x),
                               Mathf.Clamp(position.y, min.y, max.y));
        }

        private void UpdateJoystick(Vector2 position)
        {
            if (EventSystem.current == null)
            {
                Debug.LogError("EventSystem.current is null");
                return;
            }
            PointerEventData eventData = new PointerEventData(EventSystem.current) { position = position };
            raycastResults.Clear();
            EventSystem.current.RaycastAll(eventData, raycastResults);
            bool blockingUIObjectHit = raycastResults.Count > 0;
            if (blockingUIObjectHit) return;

            shouldUpdateJoystick = false;
            transform.position = position;
            touchDelta.transform.localPosition = Vector3.zero;
            Enable();
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
            canvasGroup.alpha = 0f;
            Player.Instance.SetDirection(Vector2.zero);
        }

        private void Enable()
        {
            canvasGroup.alpha = 1f;
        }

        internal void ChangePosition(Vector2 position)
        {
            if (EventSystem.current.currentSelectedGameObject != null) return;

            if (shouldUpdateJoystick) UpdateJoystick(position);
            else UpdateTouchDelta(position);
        }
    }
}
