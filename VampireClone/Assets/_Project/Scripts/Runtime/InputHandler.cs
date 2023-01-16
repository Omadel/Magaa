using Etienne;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Magaa
{
    [DefaultExecutionOrder(-1)]
    public class InputHandler : Singleton<InputHandler>
    {
        public event Action OnTouchPressed, OnTouchReleased;
        public event Action<Vector2> OnPositionChanged;
        public event Action<Vector2> OnDirectionChanged;

        public Vector2 Position => position;

        private Vector2 position;
        private List<RaycastResult> raycastResults = new List<RaycastResult>();
        private PointerEventData pointerData;

        private void Start()
        {
            pointerData = new PointerEventData(EventSystem.current);
        }

        private void OnTouchPress(InputValue input)
        {
            bool isPressed = input.Get<float>() > 0f;
            if (!isPressed)
            {
                OnTouchReleased?.Invoke();
            }
            else
            {
                EventSystem current = EventSystem.current;
                if (current != null && current.alreadySelecting) return;
                OnTouchPressed?.Invoke();
            }
        }
        private void OnTouchPosition(InputValue input)
        {
            position = input.Get<Vector2>();
            OnPositionChanged?.Invoke(position);
        }

        private void OnMove(InputValue input)
        {
            SetDirection(input.Get<Vector2>());
        }

        public void SetDirection(Vector2 uiDirection)
        {
            OnDirectionChanged?.Invoke(uiDirection);
        }

        public bool IsOverUI()
        {
            if (EventSystem.current == null) return false;
            pointerData.position = position;
            EventSystem.current.RaycastAll(pointerData, raycastResults);
            bool blockingUIObjectHit = raycastResults.Count > 0;
            return blockingUIObjectHit;
        }
    }
}
