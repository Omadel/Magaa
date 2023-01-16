using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Magaa
{
    [RequireComponent(typeof(LayoutElement))]
    public class MainMenuTab : Selectable, IPointerClickHandler, IEventSystemHandler, ISubmitHandler
    {
        [SerializeField] private LayoutElement selectedLayout;
        [SerializeField] private GameObject[] activesWhenSelected;

        protected override void Start()
        {
            selectedLayout.enabled = false;
            foreach (GameObject go in activesWhenSelected)
            {
                go.SetActive(false);
            }
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            selectedLayout.enabled = true;
            foreach (GameObject go in activesWhenSelected)
            {
                go.SetActive(true);
            }
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            Debug.Log(EventSystem.current.currentSelectedGameObject);
            selectedLayout.enabled = false;
            foreach (GameObject go in activesWhenSelected)
            {
                go.SetActive(false);
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
        }
        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
        }
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
        }
        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log("Click");
        }

        public void OnSubmit(BaseEventData eventData)
        {
            Debug.Log("Submit");
        }
    }
}
