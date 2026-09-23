using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class BirdController : MonoBehaviour
{
    [Header("Flight Settings")]
    public float FlapForce = 5.5f;
    public float MaxDownwardSpeed = -10f;
    public float UpwardTiltAngle = 30f;
    public float DownwardTiltAngle = -85f;
    public float TiltSpeed = 8f;

    [Header("State")]
    public bool IsAlive = true;
    private bool isFlying = false;

    private Rigidbody2D rb;
    private Vector3 initialPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        initialPosition = transform.position;
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.CurrentState == GameManager.GameState.Ready)
        {
            // Bob gently up and down waiting for first flap
            float bobOffset = Mathf.Sin(Time.time * 5f) * 0.15f;
            transform.position = initialPosition + new Vector3(0f, bobOffset, 0f);

            if (CheckJumpInput())
            {
                GameManager.Instance.StartGame();
                Flap();
            }
        }
        else if (GameManager.Instance.CurrentState == GameManager.GameState.Playing && IsAlive)
        {
            if (CheckJumpInput())
            {
                Flap();
            }

            UpdateRotation();
        }
        else if (GameManager.Instance.CurrentState == GameManager.GameState.GameOver)
        {
            if (CheckJumpInput())
            {
                GameManager.Instance.RestartGame();
            }
        }
    }

    private void FixedUpdate()
    {
        if (rb != null && isFlying && IsAlive)
        {
            // Clamp falling speed
#if UNITY_6000_0_OR_NEWER
            if (rb.linearVelocity.y < MaxDownwardSpeed)
            {
                rb.linearVelocity = new Vector2(0f, MaxDownwardSpeed);
            }
#else
            if (rb.velocity.y < MaxDownwardSpeed)
            {
                rb.velocity = new Vector2(0f, MaxDownwardSpeed);
            }
#endif
        }
    }

    public void SetReadyMode()
    {
        IsAlive = true;
        isFlying = false;
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector2.zero;
#else
            rb.velocity = Vector2.zero;
#endif
        }
        transform.position = initialPosition;
        transform.rotation = Quaternion.identity;
    }

    public void StartFlying()
    {
        IsAlive = true;
        isFlying = true;
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    public void Flap()
    {
        if (!IsAlive) return;

        if (rb != null)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = new Vector2(0f, FlapForce);
#else
            rb.velocity = new Vector2(0f, FlapForce);
#endif
        }

        transform.rotation = Quaternion.Euler(0f, 0f, UpwardTiltAngle);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayFlapSound();
        }
    }

    private void UpdateRotation()
    {
        if (rb == null) return;

#if UNITY_6000_0_OR_NEWER
        float currentVy = rb.linearVelocity.y;
#else
        float currentVy = rb.velocity.y;
#endif
        float targetAngle = (currentVy > 0f) ? UpwardTiltAngle : DownwardTiltAngle;
        float currentAngle = transform.eulerAngles.z;
        if (currentAngle > 180f) currentAngle -= 360f;

        float newAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * TiltSpeed);
        transform.rotation = Quaternion.Euler(0f, 0f, newAngle);
    }

    public void OnDeath()
    {
        IsAlive = false;
        transform.rotation = Quaternion.Euler(0f, 0f, -90f);
    }

    private bool CheckJumpInput()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame))
            return true;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            return true;
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            return true;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            return true;
#endif

        return false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsAlive) return;

        if (collision.gameObject.CompareTag("Obstacle") || collision.gameObject.CompareTag("Ground"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!IsAlive) return;

        if (collider.CompareTag("ScoreTrigger"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore();
            }
        }
    }
}
