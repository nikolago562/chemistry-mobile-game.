using UnityEngine;

namespace ChemistryGame
{
    [RequireComponent(typeof(Camera))]
    public sealed class ClassroomCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float smoothing = 6f;
        [SerializeField] private float zoom = 3.6f;
        [SerializeField] private bool limitToClassroom;
        [SerializeField] private Vector2 minimum;
        [SerializeField] private Vector2 maximum;

        private Camera view;
        private Vector2 position;
        private bool homeLimit;
        private bool originalLimit;
        private Vector2 homeMinimum, homeMaximum;
        private Transform focus;
        private Vector2 focusOffset;
        private float focusZoom;
        private float currentZoom;

        /// <summary>Smoothly zooms in on a speaker (e.g. the teacher) until <see cref="Release"/>.</summary>
        public void Focus(Transform speaker, Vector2 offset, float zoomLevel)
        {
            focus = speaker; focusOffset = offset; focusZoom = zoomLevel;
        }
        public void Release() => focus = null;

        public void SetRoom(Vector2 min, Vector2 max, bool personal)
        {
            if (personal && !homeLimit) { homeMinimum = minimum; homeMaximum = maximum; originalLimit = limitToClassroom; }
            if (personal) { minimum = min; maximum = max; }
            else { minimum = homeMinimum; maximum = homeMaximum; }
            homeLimit = personal;
            limitToClassroom = personal || originalLimit;
            position = target.position;
            UpdateView(1);
        }

        private void Awake()
        {
            view = GetComponent<Camera>();
            currentZoom = zoom;
            position = target.position;
            UpdateView(1f);
        }

        private void LateUpdate() => UpdateView(1f - Mathf.Exp(-smoothing * Time.deltaTime));

        private void UpdateView(float blend)
        {
            bool focused = focus != null && focus.gameObject.activeInHierarchy;
            float wantedZoom = focused ? focusZoom : zoom;
            currentZoom = Mathf.Abs(currentZoom - wantedZoom) < 0.002f ? wantedZoom : Mathf.Lerp(currentZoom, wantedZoom, blend);
            float halfHeight = Mathf.Max(648f, 1152f / view.aspect) / (32f * currentZoom);
            view.orthographicSize = halfHeight;
            Vector2 goal = focused ? (Vector2)focus.position + focusOffset : (Vector2)target.position;
            var next = Vector2.Lerp(position, goal, blend);
            if (limitToClassroom)
            {
                float halfWidth = halfHeight * view.aspect;
                next.x = Fit(next.x, minimum.x, maximum.x, halfWidth);
                next.y = Fit(next.y, minimum.y, maximum.y, halfHeight);
            }

            position = next;
            // Rendering from whole screen pixels keeps pixel art stable: without this, sub-pixel camera
            // motion makes rows of texels on benches and floors flicker in colour while walking.
            float pixel = 2 * halfHeight / Mathf.Max(1, view.pixelHeight);
            var snapped = new Vector2(Mathf.Round(position.x / pixel) * pixel, Mathf.Round(position.y / pixel) * pixel);
            transform.position = new Vector3(snapped.x, snapped.y, transform.position.z);
        }

        private static float Fit(float value, float min, float max, float halfView) =>
            min + halfView <= max - halfView
                ? Mathf.Clamp(value, min + halfView, max - halfView)
                : (min + max) * 0.5f;
    }
}
