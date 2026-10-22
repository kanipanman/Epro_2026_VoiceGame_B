
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerHidingState : MonoBehaviour
{
    [Header("ロッカー使用制限")]
    [SerializeField, Min(0.1f)]
    private float maxHideTime = 5f;

    [SerializeField, Min(0f)]
    private float cooldownTime = 12f;

    private PlayerController player;
    private Collider2D playerCollider;
    private SpriteRenderer[] visuals;

    // 隠れているか
    public bool IsHidden { get; private set; }

    // 隠れ始めた時刻
    private float hideStartTime;

    // 次に使用可能になる時刻
    private float nextAvailableTime;

    // ロッカーが使用可能か
    public bool CanUseLocker =>
        !IsHidden && Time.time >= nextAvailableTime;

    // 隠れられる残り時間
    public float RemainingHideTime =>
        IsHidden
            ? Mathf.Max(0f, maxHideTime -
                (Time.time - hideStartTime))
            : 0f;

    // クールタイムの残り時間
    public float CooldownRemaining =>
        Mathf.Max(0f, nextAvailableTime - Time.time);

    // 使用時間を超過したか
    public bool IsHideTimeExpired =>
        IsHidden && RemainingHideTime <= 0f;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        playerCollider = GetComponent<Collider2D>();

        visuals = GetComponentsInChildren<SpriteRenderer>(true);
    }

    // ロッカーへの出入り
    public void SetHidden(bool hidden)
    {
        if (IsHidden == hidden)
            return;

        // クールタイム中は隠れられない
        if (hidden && !CanUseLocker)
            return;

        IsHidden = hidden;

        if (hidden)
        {
            // 使用時間の計測を開始
            hideStartTime = Time.time;
        }
        else
        {
            // ロッカーから出た時点でクールタイム開始
            nextAvailableTime = Time.time + cooldownTime;
        }

        // 隠れている間は移動できない
        player.SetMovementEnabled(!hidden);

        // プレイヤーの見た目を切り替える
        foreach (SpriteRenderer visual in visuals)
        {
            visual.enabled = !hidden;
        }

        // 隠れている間は当たり判定を無効化
        if (playerCollider != null)
        {
            playerCollider.enabled = !hidden;
        }
    }
}
