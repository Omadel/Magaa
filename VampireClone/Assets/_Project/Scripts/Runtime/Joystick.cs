using Etienne;
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
            Debug.Log("Touch delta");
            Vector3 direction = transform.position.Direction(position).normalized.Multiply((Vector3)rectTransform.rect.size * .5f);
            Vector2 normalizedPosition = transform.position + direction;
            Vector2 invertedNormalizedPosition = transform.position - direction;
            Vector2 min = new Vector2(Mathf.Min(invertedNormalizedPosition.x, normalizedPosition.x),
                Mathf.Min(invertedNormalizedPosition.y, normalizedPosition.y));
            Vector2 max = new Vector2(Mathf.Max(invertedNormalizedPosition.x, normalizedPosition.x),
                Mathf.Max(invertedNormalizedPosition.y, normalizedPosition.y));
            position.x = Mathf.Clamp(position.x, min.x, max.x);
            position.y = Mathf.Clamp(position.y, min.y, max.y);
            touchDelta.transform.position = position;

            Player.Instance.SetDirection(direction);
        }

        private void UpdateJoystick(Vector2 position)
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current) { position = position };
            List<RaycastResult> raycsastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raycsastResults);
            Debug.Log(raycsastResults.Count);
            if (raycsastResults.Count > 0) return;

            shouldUpdateJoystick = false;
            transform.position = position;
            touchDelta.transform.localPosition = Vector3.zero;
            Enable();
        }

        public void Press(InputValue input)
        {
            if(EventSystem.current.alreadySelecting) return;
            bool isPressed = input.Get<float>() > 0f;
            if (isPressed) shouldUpdateJoystick = true;
            else Disable();
        }

        private void Disable()
        {
            Debug.Log("Diasabler");
            canvasGroup.alpha = 0f;
            Player.Instance.SetDirection(Vector2.zero);
        }

        private void Enable()
        {
            canvasGroup.alpha = 1f;
        }

        internal void ChangePosition(Vector2 position)
        {
            Debug.Log($"Already selecting {EventSystem.current.currentSelectedGameObject}");
            if (EventSystem.current.currentSelectedGameObject!=null) return;

            if (shouldUpdateJoystick) UpdateJoystick(position);
            else UpdateTouchDelta(position);
        }
    }
}
