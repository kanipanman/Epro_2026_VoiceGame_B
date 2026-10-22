
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerFootstepNoise : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private KillerController killer;
    [SerializeField] private GameManager gameManager;

    [Header("足音の設定")]
    [SerializeField, Min(0f)]
    private float hearingRange = 5f;

    [SerializeField, Min(0.1f)]
    private float noiseInterval = 0.6f;

    private PlayerController player;
    private float nextNoiseTime;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    private void Start()
    {
        if (killer == null || gameManager == null)
        {
            Debug.LogError(
                $"{name}: KillerまたはGameManagerが未設定です。"
            );
            enabled = false;
        }
    }

    private void Update()
    {
        if (gameManager == null || gameManager.HasEnded || gameManager.IsPaused)
            return;

        // ダッシュしていなければ足音を通知しない
        if (!player.IsSprinting)
            return;

        if (Time.time < nextNoiseTime)
            return;

        nextNoiseTime = Time.time + noiseInterval;

        // キラーとの距離を取得
        float distance = Vector2.Distance(
            transform.position,
            killer.transform.position
        );

        // 足音が届く距離であれば調査を指示
        if (distance <= hearingRange)
        {
            killer.HearNoise(transform.position);
        }
    }
}
