
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("移動速度")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float runSpeed = 5.0f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isRunning;

    // プレイヤーの移動可否
    private bool canMove = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (!canMove)
        {
            moveInput = Vector2.zero;
            isRunning = false;
            return;
        }

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            moveInput = Vector2.zero;
            isRunning = false;
            return;
        }

        // WASD・方向キーの入力を取得
        float x = 0f;
        float y = 0f;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            x -= 1f;

        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            x += 1f;

        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            y -= 1f;

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            y += 1f;

        // 斜め移動時の速度を一定にする
        moveInput = new Vector2(x, y).normalized;

        // Shiftキーによるダッシュ判定
        isRunning = keyboard.leftShiftKey.isPressed ||
                    keyboard.rightShiftKey.isPressed;
    }

    private void FixedUpdate()
    {
        if (!canMove) return;

        float speed = isRunning ? runSpeed : walkSpeed;

        // Rigidbody2Dによる移動
        Vector2 nextPosition = rb.position +
            moveInput * speed * Time.fixedDeltaTime;

        rb.MovePosition(nextPosition);
    }

    // 移動の有効・無効を切り替える
    public void SetMovementEnabled(bool enabled)
    {
        canMove = enabled;

        if (!enabled)
        {
            moveInput = Vector2.zero;
            isRunning = false;
            rb.linearVelocity = Vector2.zero;
        }
    }
}
