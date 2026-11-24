using UnityEngine;
using UnityEngine.EventSystems;

namespace ChemistryGame
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
    public sealed class PlayerController : MonoBehaviour
    {
        public bool canMove = true;
        [SerializeField] private float speed = 5.25f; // quicker than the first 4.25 so walks between tables stay short
        [SerializeField] private float acceleration = 28f;
        [SerializeField] private float deceleration = 40f;
        private static readonly int MoveX = Animator.StringToHash("MoveX");
        private static readonly int MoveY = Animator.StringToHash("MoveY");
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");
        private Vector2 previousFeet;
        private float strideDistance;
        public Vector2 Facing { get; private set; } = Vector2.down;
        public Vector2 Movement { get; private set; }
        private Rigidbody2D body;
        private Animator animator;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            previousFeet = body.position;
        }
        private void Update()
        {
            canMove = !LabUI.AnyOpen;
            SetMovement(canMove ? Steer() : Vector2.zero);
            if (!canMove) body.linearVelocity = Vector2.zero;
            // Use displacement after collision resolution: pushing against a wall is not walking.
            float distance = Vector2.Distance(body.position, previousFeet);
            previousFeet = body.position;
            if (distance < speed * 0.1f) strideDistance += distance; // Ignore room teleports.
            bool walking = canMove && body.linearVelocity.sqrMagnitude > 0.04f;
            animator.SetFloat(MoveX, Facing.x);
            animator.SetFloat(MoveY, Facing.y);
            animator.SetBool(IsMoving, walking);
            animator.speed = walking ? Mathf.Clamp(body.linearVelocity.magnitude / speed, 0.4f, 1.25f) : 1;
            if (walking && strideDistance >= 1.05f)
            {
                strideDistance %= 1.05f;
                AudioManager.Instance?.Step();
            }
            if (!walking) strideDistance = 0;
        }
        // Tap-to-walk: a tap on the world walks there; a tap on (or near) something usable walks to it and uses it.
        private Vector2? walkTo;
        private Focus walkFocus;
        private float stuck;
        private Vector2 Steer()
        {
            var manual = ReadMovement();
            var dialogue = LabGame.Instance != null ? LabGame.Instance.Dialogue : null;
            if (TappedWorld(out var point) && (dialogue == null || !dialogue.IsOpen))
            {
                walkFocus = Focus.At(point);
                walkTo = walkFocus != null ? walkFocus.Spot : point;
                Focus.Tapped = walkFocus;
                stuck = 0;
            }
            if (manual != Vector2.zero || walkTo == null) { if (manual != Vector2.zero) StopWalk(); return manual; }
            if (walkFocus != null && walkFocus.IsFocused) { TouchControls.Press(); StopWalk(); return Vector2.zero; }
            var offset = walkTo.Value - body.position;
            // shortcut: walks in a straight line and gives up when blocked; add pathfinding if rooms get maze-like.
            stuck = body.linearVelocity.sqrMagnitude < 0.5f ? stuck + Time.deltaTime : 0;
            if (offset.magnitude < 0.2f || stuck > 0.45f) { StopWalk(); return Vector2.zero; }
            return offset.normalized;
        }
        private void StopWalk() { walkTo = null; walkFocus = null; Focus.Tapped = null; }

        private static bool TappedWorld(out Vector2 point)
        {
            point = default;
            var camera = Camera.main;
            if (!Input.GetMouseButtonDown(0) || camera == null) return false;
            var events = EventSystem.current;
            if (events != null && (events.IsPointerOverGameObject() || Input.touchCount > 0 && events.IsPointerOverGameObject(Input.GetTouch(0).fingerId))) return false;
            point = camera.ScreenToWorldPoint(Input.mousePosition);
            return true;
        }

        public void SetMovement(Vector2 direction)
        {
            Movement = Vector2.ClampMagnitude(direction, 1);
            if (Movement != Vector2.zero) Facing = Movement.normalized;
        }
        private void FixedUpdate()
        {
            if (!canMove || LabUI.AnyOpen) { body.linearVelocity = Vector2.zero; return; }
            float rate = Movement == Vector2.zero ? deceleration : acceleration;
            body.linearVelocity = Vector2.MoveTowards(body.linearVelocity, Movement * speed, rate * Time.fixedDeltaTime);
        }
        public static Vector2 ReadMovement()
        {
            if (TouchControls.Move != Vector2.zero) return TouchControls.Move;
            float x = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0)
                - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1 : 0);
            float y = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1 : 0)
                - (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1 : 0);
            return new Vector2(x, y).normalized;
        }
    }
}
