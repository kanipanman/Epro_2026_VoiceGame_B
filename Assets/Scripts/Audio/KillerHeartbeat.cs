
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class KillerHeartbeat : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform killer;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private AudioClip heartbeatClip;

    [Header("心音が聞こえる距離")]
    [SerializeField] private float nearDistance = 2f;
    [SerializeField] private float farDistance = 10f;

    [Header("心音の間隔")]
    [SerializeField] private float slowInterval = 1.2f;
    [SerializeField] private float fastInterval = 0.42f;

    [Header("最大音量")]
    [SerializeField, Range(0f, 1f)]
    private float maxVolume = 0.9f;

    private AudioSource audioSource;
    private float heartbeatTimer;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 1f;
    }

    private void Update()
    {
        if (player == null || killer == null ||
            heartbeatClip == null || gameManager == null)
            return;

        // ゲーム終了時は心音を停止
        if (gameManager.HasEnded)
        {
            audioSource.Stop();
            return;
        }

        float distance = Vector2.Distance(
            player.position,
            killer.position
        );

        // キラーが遠い場合は心音を再生しない
        if (distance >= farDistance)
        {
            heartbeatTimer = 0f;
            return;
        }

        // 距離に応じた危険度（0～1）
        float danger = Mathf.Clamp01(
            (farDistance - distance) /
            Mathf.Max(0.1f, farDistance - nearDistance)
        );

        // キラーが近いほど心音が速くなる
        float interval = Mathf.Lerp(
            slowInterval,
            fastInterval,
            danger
        );

        // キラーが近いほど音量が大きくなる
        float volume = danger * maxVolume;

        heartbeatTimer -= Time.deltaTime;

        // 心音を一定間隔で再生
        if (heartbeatTimer <= 0f)
        {
            audioSource.PlayOneShot(heartbeatClip, volume);
            heartbeatTimer = interval;
        }
    }

    private void OnDisable()
    {
        if (audioSource != null)
            audioSource.Stop();
    }
}
