
using UnityEngine;

[RequireComponent(typeof(KillerController))]
public class KillerHearing : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private MicrophoneInput microphone;
    [SerializeField] private Transform player;

    [Header("音量の判定")]
    [SerializeField, Range(0f, 1f)]
    private float minimumVolume = 0.2f;

    [SerializeField, Range(0f, 1f)]
    private float loudVolume = 0.7f;

    [Header("聴覚範囲")]
    [SerializeField] private float minimumHearingRange = 3.5f;
    [SerializeField] private float maximumHearingRange = 10f;

    [Header("検知間隔")]
    [SerializeField] private float detectionInterval = 0.7f;

    private KillerController killerController;

    // プレイヤーの隠れ状態
    private PlayerHidingState hidingState;

    private float nextDetectionTime;

    private void Awake()
    {
        killerController = GetComponent<KillerController>();
    }

    private void Start()
    {
        if (microphone == null || player == null)
        {
            Debug.LogError(
                $"{name}: マイクまたはプレイヤーが未設定です。"
            );

            enabled = false;
            return;
        }

        // プレイヤーのロッカー状態を取得
        hidingState = player.GetComponent<PlayerHidingState>();

        if (hidingState == null)
        {
            Debug.LogWarning(
                "PlayerHidingStateが見つかりません。"
            );
        }
    }

    private void Update()
    {
        // ポーズ中は音を検知しない
        if (Time.timeScale <= 0f)
            return;

        if (microphone == null ||
            !microphone.IsReady ||
            player == null)
            return;

        // ロッカー内では声を検知しない
        if (hidingState != null && hidingState.IsHidden)
            return;

        float volume = microphone.Volume01;

        // 最低音量未満なら無視
        if (volume < minimumVolume)
            return;

        // 声量によって聴覚範囲を決定
        float t = Mathf.InverseLerp(
            minimumVolume,
            loudVolume,
            volume
        );

        float hearingRange = Mathf.Lerp(
            minimumHearingRange,
            maximumHearingRange,
            t
        );

        // キラーとプレイヤーの距離
        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        if (distance > hearingRange)
            return;

        // 検知間隔を制限
        if (Time.time < nextDetectionTime)
            return;

        nextDetectionTime = Time.time +
            Mathf.Max(0.1f, detectionInterval);

        // 声がした位置をキラーへ通知
        killerController.HearNoise(player.position);
    }

    // シーンビューで聴覚範囲を表示
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            maximumHearingRange
        );
    }
}
