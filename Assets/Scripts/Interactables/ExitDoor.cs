
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class ExitDoor : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private GeneratorManager generatorManager;
    [SerializeField] private GameManager gameManager;

    [Header("出口の色")]
    [SerializeField] private Color lockedColor = Color.red;
    [SerializeField] private Color unlockedColor = Color.green;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D doorCollider;

    private bool isUnlocked;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        doorCollider = GetComponent<BoxCollider2D>();

        // 最初は閉じている
        isUnlocked = false;

        doorCollider.isTrigger = false;
        spriteRenderer.color = lockedColor;
    }

    private void Start()
    {
        if (generatorManager == null || gameManager == null)
        {
            Debug.LogError(
                $"{name}: 必要な参照が設定されていません。"
            );

            enabled = false;
            return;
        }

        // 全装置の起動完了を受け取る
        generatorManager.OnAllActivated += UnlockDoor;

        if (generatorManager.AllActivated)
        {
            UnlockDoor();
        }
    }

    // すべての装置が起動したら開放
    private void UnlockDoor()
    {
        if (isUnlocked || gameManager.HasEnded)
            return;

        isUnlocked = true;

        doorCollider.isTrigger = true;
        spriteRenderer.color = unlockedColor;

        Debug.Log("出口が開放されました！");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryEscape(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryEscape(other);
    }

    // プレイヤーが出口に触れたか確認
    private void TryEscape(Collider2D other)
    {
        if (!isUnlocked || gameManager == null ||
            gameManager.HasEnded)
            return;

        PlayerController player =
            other.GetComponentInParent<PlayerController>();

        if (player == null)
            return;

        // ゲームクリアを通知
        gameManager.GameClear();
    }

    private void OnDestroy()
    {
        if (generatorManager != null)
        {
            generatorManager.OnAllActivated -= UnlockDoor;
        }
    }
}
