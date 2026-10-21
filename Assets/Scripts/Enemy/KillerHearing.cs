
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
        }
    }

    private void Update()
    {
        // マイクが使用可能でなければ処理しない
        if (microphone == null ||
            !microphone.IsReady ||
            player == null)
            return;

        float volume = microphone.Volume01;

        // 必要な音量に達していなければ無視
        if (volume < minimumVolume)
            return;

        // 音量からキラーの聴覚範囲を計算
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

        // 聴覚範囲外なら聞こえない
        if (distance > hearingRange)
            return;

        // 毎フレーム通知しないよう間隔を設ける
        if (Time.time < nextDetectionTime)
            return;

        nextDetectionTime = Time.time +
            Mathf.Max(0.1f, detectionInterval);

        // 声がした時点の位置を通知
        killerController.HearNoise(player.position);
    }

    // シーンビューに最大聴覚範囲を表示
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            maximumHearingRange
        );
    }
}
