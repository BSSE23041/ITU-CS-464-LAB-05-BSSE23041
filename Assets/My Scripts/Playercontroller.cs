
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveForce = 10f;
    [SerializeField] ForceMode mode = ForceMode.Force;

    [Header("Controls")]
    [Tooltip("Movement based on camera direction")]
    [SerializeField] bool cameraRelative = true;
    [SerializeField] Transform cam;

    Rigidbody rb;
    Vector3 input;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (cam == null && Camera.main != null) cam = Camera.main.transform;
    }

    // get player movement
    void Update()
    {
        Vector2 raw = ReadInput();
        Vector3 move = new Vector3(raw.x, 0f, raw.y);

        if (cameraRelative && cam != null)
        {
            Vector3 fwd = cam.forward;
            fwd.y = 0f;
            if (fwd.sqrMagnitude < 0.01f)
            {
                fwd = cam.up;
                fwd.y = 0f;
            }
            fwd.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, fwd);
            move = fwd * raw.y + right * raw.x;
        }

        input = Vector3.ClampMagnitude(move, 1f);
    }

    // move ball using force
    void FixedUpdate() => rb.AddForce(input * moveForce, mode);

    Vector2 ReadInput()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        Keyboard k = Keyboard.current;
        if (k == null) return Vector2.zero;
        float h = ((k.dKey.isPressed || k.rightArrowKey.isPressed) ? 1f : 0f)
                - ((k.aKey.isPressed || k.leftArrowKey.isPressed) ? 1f : 0f);
        float v = ((k.wKey.isPressed || k.upArrowKey.isPressed) ? 1f : 0f)
                - ((k.sKey.isPressed || k.downArrowKey.isPressed) ? 1f : 0f);
        return new Vector2(h, v);
#else
        return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
#endif
    }
}
