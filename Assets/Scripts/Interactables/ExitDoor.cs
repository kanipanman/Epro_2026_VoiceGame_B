
using TMPro;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class ExitDoor : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private GeneratorManager generatorManager;
    [SerializeField] private TMP_Text clearText;

    [Header("出口の色")]
    [SerializeField] private Color lockedColor = Color.red;
    [SerializeField] private Color unlockedColor = Color.green;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D doorCollider;

    // 出口の状態
    private bool isUnlocked;
    private bool isCleared;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        doorCollider = GetComponent<BoxCollider2D>();

        // 最初は閉鎖状態
        isUnlocked = false;
        doorCollider.isTrigger = false;
        spriteRenderer.color = lockedColor;

        // クリア表示を非表示にする
        if (clearText != null)
        {
            clearText.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        if (generatorManager == null)
        {
            Debug.LogError(
                $"{name}: GeneratorManagerが未設定です。"
            );
            enabled = false;
            return;
        }

        // 装置がすべて起動した通知を受け取る
        generatorManager.OnAllActivated += UnlockDoor;

        // すでに条件を満たしている場合にも対応
        if (generatorManager.AllActivated)
        {
            UnlockDoor();
        }
    }

    // 出口を開放
    private void UnlockDoor()
    {
        if (isUnlocked) return;

        isUnlocked = true;

        // プレイヤーが通れるようにする
        doorCollider.isTrigger = true;
        spriteRenderer.color = unlockedColor;

        Debug.Log("出口が開放されました！");
    }

    // 出口に接触したときの処理
    private void OnTriggerEnter2D(Collider2D other)
    {
        TryEscape(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryEscape(other);
    }

    // 脱出判定
    private void TryEscape(Collider2D other)
    {
        if (!isUnlocked || isCleared)
            return;

        // プレイヤー以外は反応しない
        if (other.GetComponent<PlayerController>() == null)
            return;

        isCleared = true;

        Debug.Log("脱出成功！");

        if (clearText != null)
        {
            clearText.text = "脱出成功！";
            clearText.gameObject.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        // イベントの購読を解除
        if (generatorManager != null)
        {
            generatorManager.OnAllActivated -= UnlockDoor;
        }
    }
}
