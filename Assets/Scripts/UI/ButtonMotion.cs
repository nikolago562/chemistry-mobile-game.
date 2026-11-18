using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ChemistryGame
{
    public sealed class ButtonMotion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
    {
        private float target = 1;
        private bool hovering;
        private Selectable control;
        private void Awake() => control = GetComponent<Selectable>();
        public void OnPointerEnter(PointerEventData data) { hovering = true; target = 1.025f; }
        public void OnPointerExit(PointerEventData data) { hovering = false; target = 1; }
        public void OnPointerDown(PointerEventData data) => target = 0.97f;
        public void OnPointerUp(PointerEventData data) => target = hovering ? 1.025f : 1;
        public void OnSelect(BaseEventData data) => target = 1.02f;
        public void OnDeselect(BaseEventData data) => target = 1;
        private void Update()
        {
            if (!control.interactable) target = 1;
            var desired = Vector3.one * target;
            if (transform.localScale == desired) return;
            var next = Vector3.Lerp(transform.localScale, desired, 1 - Mathf.Exp(-18 * Time.unscaledDeltaTime));
            transform.localScale = (next - desired).sqrMagnitude < 0.000001f ? desired : next;
        }
    }
}
