
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
public class LockerHiding : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private PlayerHidingState player;
    [SerializeField] private KillerController killer;
    [SerializeField] private GameManager gameManager;

    [Header("操作設定")]
    [SerializeField] private float interactionRange = 1.5f;

    [Header("表示色")]
    [SerializeField] private Color normalColor = Color.gray;
    [SerializeField] private Color occupiedColor = Color.cyan;

    [SerializeField] private Color cooldownColor =
        new Color(0.85f, 0.85f, 0.85f, 0.55f);

    private SpriteRenderer spriteRenderer;

    // 現在使用中のロッカー
    private static LockerHiding activeLocker;

    // 再生開始時に静的状態を初期化
    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetLockerState()
    {
        activeLocker = null;
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = normalColor;
    }

    private void Update()
    {
        if (player == null || killer == null ||
            gameManager == null)
            return;

        // 状態に応じてロッカーの色を更新
        UpdateLockerColor();

        if (gameManager.HasEnded || gameManager.IsPaused)
            return;

        // 使用時間を超えたら強制退出
        if (activeLocker == this &&
            player.IsHideTimeExpired)
        {
            Debug.Log("ロッカーの使用時間が終了！");
            ExitLocker(true);
            return;
        }

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null ||
            !keyboard.eKey.wasPressedThisFrame)
            return;

        // 使用中のロッカーから退出
        if (activeLocker == this)
        {
            ExitLocker(false);
            return;
        }

        // 他のロッカーを使用している
        if (activeLocker != null)
            return;

        // クールタイム中は使用できない
        if (!player.CanUseLocker)
            return;

        // プレイヤーとの距離判定
        float distance = Vector2.Distance(
            transform.position,
            player.transform.position
        );

        if (distance > interactionRange)
            return;

        // キラーから追跡されている間は入れない
        if (killer.State ==
            KillerController.KillerState.Chase)
            return;

        EnterLocker();
    }

    // ロッカーに入る
    private void EnterLocker()
    {
        if (!player.CanUseLocker)
            return;

        player.SetHidden(true);

        if (!player.IsHidden)
            return;

        activeLocker = this;

        UpdateLockerColor();

        Debug.Log("ロッカーに隠れた");
    }

    // ロッカーから出る
    private void ExitLocker(bool forced)
    {
        if (activeLocker != this)
            return;

        // 通常退出時はキラーとの距離を確認
        if (!forced)
        {
            float distance = Vector2.Distance(
                player.transform.position,
                killer.transform.position
            );

            if (distance < 1.5f)
            {
                Debug.Log("キラーが近いため出られない");
                return;
            }
        }

        // 強制退出の場合はキラーの距離に関係なく出る
        player.SetHidden(false);

        activeLocker = null;

        UpdateLockerColor();

        Debug.Log(
            forced
                ? "制限時間により強制退出"
                : "ロッカーから退出"
        );
    }

    // ロッカーの状態に応じて色を変更
    private void UpdateLockerColor()
    {
        if (spriteRenderer == null)
            return;

        if (activeLocker == this)
        {
            // 使用中は水色
            spriteRenderer.color = occupiedColor;
        }
        else if (!player.CanUseLocker)
        {
            // 使用不可の間は薄い色
            spriteRenderer.color = cooldownColor;
        }
        else
        {
            // 使用可能なときは通常色
            spriteRenderer.color = normalColor;
        }
    }

    private void OnDisable()
    {
        if (activeLocker != this)
            return;

        // オブジェクトが無効になった場合も退出
        if (player != null && player.IsHidden)
        {
            player.SetHidden(false);
        }

        activeLocker = null;
    }
}
