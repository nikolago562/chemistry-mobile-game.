using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame
{
    public sealed class FlaskMotion : MonoBehaviour
    {
        private Vector2 position;
        private RectTransform rect;
        private void Start()
        {
            rect = (RectTransform)transform;
            position = rect.anchoredPosition;
        }
        private void Update()
        {
            rect.anchoredPosition = position + Vector2.up * Mathf.Sin(Time.unscaledTime * 2.6f) * 4;
            rect.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.unscaledTime * 1.6f) * 4);
        }
    }
}
