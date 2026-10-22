
using UnityEngine;

public class GameBGMManager : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private KillerController killer;
    [SerializeField] private GameManager gameManager;

    [Header("BGM再生用")]
    [SerializeField] private AudioSource stageAudio;
    [SerializeField] private AudioSource chaseAudio;

    [Header("音量設定")]
    [SerializeField, Range(0f, 1f)]
    private float stageVolume = 0.35f;

    [SerializeField, Range(0f, 1f)]
    private float chaseVolume = 0.6f;

    [Header("切り替え設定")]
    [SerializeField, Min(0.1f)]
    private float fadeDuration = 1f;

    [SerializeField, Min(0f)]
    private float chaseHoldTime = 2f;

    // チェイス終了後のBGM維持時間
    private float chaseHoldTimer;

    private bool hasStopped;

    private void Start()
    {
        if (killer == null || gameManager == null ||
            stageAudio == null || chaseAudio == null ||
            stageAudio == chaseAudio)
        {
            Debug.LogError(
                "BGMManager：参照の設定が正しくありません。"
            );
            enabled = false;
            return;
        }

        if (stageAudio.clip == null || chaseAudio.clip == null)
        {
            Debug.LogError("BGMが設定されていません。");
            enabled = false;
            return;
        }

        // BGMの初期設定
        SetupAudio(stageAudio);
        SetupAudio(chaseAudio);

        stageAudio.volume = stageVolume;
        chaseAudio.volume = 0f;

        // 最初はステージBGMのみ再生
        stageAudio.Play();
    }

    private void SetupAudio(AudioSource source)
    {
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;
    }

    private void Update()
    {
        if (gameManager == null) return;

        // ゲーム終了時に両方のBGMを停止
        if (gameManager.HasEnded)
        {
            StopAllBGM();
            return;
        }

        // ポーズ中はBGMの切り替え処理を進めない
        if (gameManager.IsPaused)
        {
            return;
        }

        bool isChasing =
            killer.State == KillerController.KillerState.Chase;

        // 追跡状態を検知
        if (isChasing)
        {
            chaseHoldTimer = chaseHoldTime;
        }
        else
        {
            chaseHoldTimer = Mathf.Max(
                0f,
                chaseHoldTimer - Time.unscaledDeltaTime
            );
        }

        // 追跡終了直後はチェイスBGMを維持
        bool useChaseBGM =
            isChasing || chaseHoldTimer > 0f;

        if (useChaseBGM && !chaseAudio.isPlaying)
        {
            chaseAudio.Play();
        }

        // 現在の状態に応じた目標音量
        float targetStage = useChaseBGM
            ? 0f : stageVolume;

        float targetChase = useChaseBGM
            ? chaseVolume : 0f;

        // 徐々に音量を切り替える
        float fadeStep =
            Time.unscaledDeltaTime /
            Mathf.Max(0.1f, fadeDuration);

        stageAudio.volume = Mathf.MoveTowards(
            stageAudio.volume,
            targetStage,
            stageVolume * fadeStep
        );

        chaseAudio.volume = Mathf.MoveTowards(
            chaseAudio.volume,
            targetChase,
            chaseVolume * fadeStep
        );

        // フェードアウトが終わったらチェイスBGMを停止
        if (!useChaseBGM &&
            chaseAudio.isPlaying &&
            chaseAudio.volume <= 0.001f)
        {
            chaseAudio.Stop();
        }
    }

    // ゲーム終了時のBGM停止
    private void StopAllBGM()
    {
        if (hasStopped) return;

        hasStopped = true;

        stageAudio.Stop();
        chaseAudio.Stop();

        stageAudio.volume = 0f;
        chaseAudio.volume = 0f;
    }

    private void OnDisable()
    {
        if (stageAudio != null)
            stageAudio.Stop();

        if (chaseAudio != null)
            chaseAudio.Stop();
    }
}
