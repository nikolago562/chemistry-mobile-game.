using UnityEngine;

namespace ChemistryGame
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
    public sealed class PlayerController : MonoBehaviour
    {
        public bool canMove = true;
        [SerializeField] private float speed = 4.25f;
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
            SetMovement(canMove ? ReadMovement() : Vector2.zero);
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
